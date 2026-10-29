using Lab2.Api.Services;
using Lab2.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Infrastructure : DbContext Oracle + repositories
builder.Services.AddInfrastructure(builder.Configuration);

// AutoMapper : détecte automatiquement tous les profils de l'assembly Api
builder.Services.AddAutoMapper(_ => { }, typeof(Program).Assembly);

// Services (façade de la logique métier)
builder.Services.AddScoped<IClientService, ClientService>();

// Session par cookie (cas 2). Une API répond 401/403 au lieu de rediriger vers une page de login.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "webflix.session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
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
