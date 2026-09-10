using Api.Application.Queries.Pedidos;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Configurations;

public static class DependencyInjectionConfigurations
{
    public static IServiceCollection AddDependencyInjectionConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IPedidoQueries, PedidoQueries>();
        services.AddScoped<IPedidoDapperQueries, PedidoDapperQueries>();

        // /health = readiness: só responde 200 se a API sobe E consegue falar com o banco.
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>(name: "sqlserver");

        return services;
    }
}
