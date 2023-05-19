using System;
using Banking.Simulation.DataAccess.Connection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Banking.Simulation.DataAccess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccessServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString(nameof(DatabaseContext)));

                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

                if (environment.IsDevelopment())
                {
                    options.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()));
                    options.EnableSensitiveDataLogging();
                }
            }
        );

        return services;
    }
}