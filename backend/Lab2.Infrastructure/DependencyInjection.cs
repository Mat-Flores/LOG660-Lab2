using Lab2.Infrastructure.Data;
using Lab2.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lab2.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default est manquante.");

        services.AddDbContext<AppDbContext>(options => options.UseOracle(connectionString));

        services.AddScoped<IClientRepository, ClientRepository>();

        return services;
    }
}
