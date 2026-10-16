using Lab2.Api.Services;
using Lab2.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Infrastructure : DbContext Oracle + repositories
builder.Services.AddInfrastructure(builder.Configuration);

// AutoMapper : détecte automatiquement tous les profils de l'assembly Api
builder.Services.AddAutoMapper(_ => { }, typeof(Program).Assembly);

// Services (logique métier)
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IAuthentificationService, AuthentificationService>();
builder.Services.AddScoped<ICatalogueService, CatalogueService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "Lab2.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);

        // Une API retourne des codes HTTP, plutôt qu'une redirection vers une page de connexion.
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
