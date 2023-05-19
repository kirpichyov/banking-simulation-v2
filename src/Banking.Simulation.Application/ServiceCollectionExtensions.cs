using Banking.Simulation.Application.Services;
using Banking.Simulation.Application.Services.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Banking.Simulation.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}