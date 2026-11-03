using Lab2.Api.Services;
using Lab2.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAutoMapper(_ => { }, typeof(Program).Assembly);
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<ICatalogueService, CatalogueService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddControllers();

var app = builder.Build();
app.UseCors();
app.MapControllers();
app.Run();
