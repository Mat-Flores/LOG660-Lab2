using Lab2.Api.Services;
using Lab2.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Infrastructure : DbContext Oracle + repositories
builder.Services.AddInfrastructure(builder.Configuration);

// AutoMapper : détecte automatiquement tous les profils de l'assembly Api
builder.Services.AddAutoMapper(_ => { }, typeof(Program).Assembly);

// Services (logique métier)
builder.Services.AddScoped<IClientService, ClientService>();

builder.Services.AddControllers();

// CORS : origines autorisées lues dans appsettings.json (Cors:AllowedOrigins)
var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
              ?? Array.Empty<string>();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseCors();
app.MapControllers();

app.Run();
