using Microsoft.EntityFrameworkCore;
using MonApp.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// --- Base de données (EF Core) ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("Default")));

// --- Mappage Entité <-> DTO (scanne tous les Profile de l'assembly) ---
// Clé de licence optionnelle (AutoMapper 15+) : dotnet user-secrets set "AutoMapper:LicenseKey" "<clé>"
builder.Services.AddAutoMapper(
    cfg => cfg.LicenseKey = builder.Configuration["AutoMapper:LicenseKey"],
    typeof(Program));

// --- CORS : autorise le frontend (Vite par défaut) ---
const string FrontendCors = "Frontend";
builder.Services.AddCors(options => options.AddPolicy(FrontendCors, policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();

app.UseCors(FrontendCors);

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

app.Run();
