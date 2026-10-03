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

var app = builder.Build();

app.MapControllers();

app.Run();
