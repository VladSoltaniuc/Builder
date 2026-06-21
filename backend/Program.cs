using ProductApi.Services;

// Program.cs trebuie să rămână "subțire": doar configurează aplicația.
// Logica de business stă în Services, endpoint-urile în Controllers.
var builder = WebApplication.CreateBuilder(args);

// Numele politicii CORS - îl refolosim mai jos. Constantă ca să nu greșim string-ul.
const string FrontendCorsPolicy = "AllowFrontend";

// ---- Înregistrarea serviciilor în containerul de Dependency Injection (DI) ----

// Controller-ele (clase din folderul Controllers/).
builder.Services.AddControllers();

// Swagger / OpenAPI: UI interactiv pentru a testa endpoint-urile.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Înregistrăm serviciul nostru DUPĂ interfață, nu după clasa concretă.
// Astfel controller-ul depinde de abstracție (IProductService), nu de implementare.
// Singleton: păstrăm aceeași instanță (deci aceeași listă în memorie) pe tot parcursul rulării.
builder.Services.AddSingleton<IProductService, ProductService>();

// CORS: browserul blochează apelurile din frontend (alt port) către API
// dacă serverul nu permite explicit originea. Aici permitem dev server-ul Vite.
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// ---- Configurarea pipeline-ului de HTTP (ordinea contează) ----

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(FrontendCorsPolicy);
app.MapControllers();

app.Run();
