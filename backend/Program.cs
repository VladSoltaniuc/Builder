// Composition root
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using ProductApi.Auth;
using ProductApi.Contracts;
using ProductApi.Data;
using ProductApi.Hubs;
using ProductApi.Infrastructure;
using ProductApi.Maintenance;
using ProductApi.Reports;
using ProductApi.Services;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "AllowFrontend";

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddSignalR()
    .AddJsonProtocol(o => o.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<AuditUserInterceptor>();
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
           .AddInterceptors(sp.GetRequiredService<AuditUserInterceptor>()));

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();

// --- Authentication (JWT bearer) ---
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<GoogleAuthOptions>(builder.Configuration.GetSection("Authentication:Google"));
builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("App"));
builder.Services.Configure<AdminSeedOptions>(builder.Configuration.GetSection("AdminSeed"));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<ITotpService, TotpService>();

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("Missing Jwt configuration section.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // SignalR WebSocket handshakes can't set Authorization headers, so the JS client
        // sends the JWT as ?access_token= in the query string instead.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) && ctx.Request.Path.StartsWithSegments("/hubs"))
                    ctx.Token = token;
                return Task.CompletedTask;
            }
        };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
        };
    });

builder.Services.AddAuthorization();

// Background services are skipped under integration tests so they never touch the test DB.
var isTesting = builder.Environment.IsEnvironment("Testing");

builder.Services.Configure<IndexMaintenanceOptions>(builder.Configuration.GetSection("IndexMaintenance"));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<SmsOptions>(builder.Configuration.GetSection("Sms"));
builder.Services.Configure<WeeklyReportOptions>(builder.Configuration.GetSection("WeeklyReport"));
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
builder.Services.AddSingleton<ISmsSender, TwilioSmsSender>();
builder.Services.AddSingleton<IEmailQueue, EmailQueue>();

if (!isTesting)
{
    builder.Services.AddHostedService<IndexMaintenanceService>();
    builder.Services.AddHostedService<EmailQueueProcessor>();
    builder.Services.AddHostedService<WeeklyReportService>();
}

// Model validation errors → unified error envelope
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = ctx =>
    {
        var message = ctx.ModelState
            .SelectMany(x => x.Value!.Errors)
            .Select(e => e.ErrorMessage)
            .FirstOrDefault() ?? "Invalid request.";
        return new BadRequestObjectResult(
            new ErrorResponse(new ErrorDetail(400, "INVALID_ARGUMENT", message)));
    };
});

var rateLimitSection = builder.Configuration.GetSection("RateLimiting");
var isRateLimitEnabled = rateLimitSection.GetValue<bool>("Enabled");
if (isRateLimitEnabled)
{
    int permitLimit   = rateLimitSection.GetValue<int>("PermitLimit");
    int windowSeconds = rateLimitSection.GetValue<int>("WindowSeconds");

    builder.Services.AddRateLimiter(options =>
    {
        // One fixed window per client IP — resets every WindowSeconds
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window      = TimeSpan.FromSeconds(windowSeconds),
                }
            )
        );
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });
}

var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()); // required for SignalR WebSocket upgrade handshake
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(FrontendCorsPolicy);

app.UseExceptionHandler(err => err.Run(async ctx =>
{
    var ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
    int code; string status, message;
    string? detail = null;
    if (ex is UserFriendlyException ufe)
        (code, status, message, detail) = (400, ufe.ErrorCode, ufe.Message, ufe.Detail);
    // Someone else changed the row between our read and write (optimistic concurrency).
    else if (ex is DbUpdateConcurrencyException)
        (code, status, message) = (409, "CONFLICT", "This record was changed by another request. Please refresh and try again.");
    // A unique index rejected a duplicate value. Map the index to a friendly message.
    else if (ex is DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg })
        (code, status, message) = (409, "CONFLICT", pg.ConstraintName switch
        {
            "IX_Orders_Awb"        => "Another order already uses this AWB.",
            "IX_Users_Email_Unique" => "A user with this email already exists.",
            _ => "That value conflicts with an existing record."
        });
    else
        (code, status, message) = (500, "INTERNAL", "An unexpected error occurred.");
    ctx.Response.StatusCode = code;
    ctx.Response.ContentType = "application/json";
    await ctx.Response.WriteAsJsonAsync(
        new ErrorResponse(new ErrorDetail(code, status, message, detail)));
}));

if (isRateLimitEnabled)
    app.UseRateLimiter();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<OrderHub>("/hubs/orders");

// Provision the configured founder Admin (no-op unless AdminSeed is set).
await app.Services.SeedAdminAsync();

app.Run();

// Exposes Program to WebApplicationFactory in integration tests
public partial class Program { }
