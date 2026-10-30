using Lab2.Api.Services;
using Lab2.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Infrastructure : DbContext Oracle + repositories
builder.Services.AddInfrastructure(builder.Configuration);

// AutoMapper : détecte automatiquement tous les profils de l'assembly Api
builder.Services.AddAutoMapper(_ => { }, typeof(Program).Assembly);

// Services (façade de la logique métier)
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<ICatalogueService, CatalogueService>();
builder.Services.AddScoped<ILocationService, LocationService>();

// Le frontend tourne sur un autre port en développement : on accepte toutes les origines.
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors();

app.MapControllers();

app.Run();
