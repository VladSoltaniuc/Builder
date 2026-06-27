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
using ProductApi.Infrastructure;
using ProductApi.Maintenance;
using ProductApi.Services;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "AllowFrontend";

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();

// --- Authentication (JWT bearer) ---
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<GoogleAuthOptions>(builder.Configuration.GetSection("Authentication:Google"));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<ITotpService, TotpService>();

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("Missing Jwt configuration section.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
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

// Background index maintenance (bloat-gated REINDEX CONCURRENTLY).
// Skipped under integration tests so it never touches the test DB.
builder.Services.Configure<IndexMaintenanceOptions>(builder.Configuration.GetSection("IndexMaintenance"));
if (!builder.Environment.IsEnvironment("Testing"))
    builder.Services.AddHostedService<IndexMaintenanceService>();

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
if (rateLimitSection.GetValue<bool>("Enabled"))
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

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
              .AllowAnyHeader()
              .AllowAnyMethod());
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

if (rateLimitSection.GetValue<bool>("Enabled"))
    app.UseRateLimiter();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposes Program to WebApplicationFactory in integration tests
public partial class Program { }
