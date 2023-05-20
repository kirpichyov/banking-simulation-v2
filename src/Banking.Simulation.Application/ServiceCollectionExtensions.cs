using Banking.Simulation.Application.Consumers;
using Banking.Simulation.Application.Jobs;
using Banking.Simulation.Application.Quartz;
using Banking.Simulation.Application.Services;
using Banking.Simulation.Application.Services.Contracts;
using Banking.Simulation.Core.Options;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Banking.Simulation.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IWebhookConfigsService, WebhookConfigsService>();
        services.AddScoped<IPaymentsService, PaymentsService>();
        services.Configure<SimulationOptions>(configuration.GetSection(nameof(SimulationOptions)));

        return services;
    }

    public static IServiceCollection AddBroker(this IServiceCollection services)
    {
        services.AddMassTransit(configurator =>
        {
            configurator.AddConsumer(typeof(SendWebhookCommandConsumer));
            
            configurator.UsingInMemory((context, config) =>
            {
                config.ConfigureEndpoints(context);
            });
        });

        return services;
    }
    
    public static IServiceCollection AddJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddQuartz(quartz =>
        {
            using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
                .SetMinimumLevel(LogLevel.Information)
                .AddConsole());

            var logger = loggerFactory.CreateLogger<QuartzLogProvider>();
        
            LogProvider.SetCurrentLogProvider(new QuartzLogProvider(logger));
            quartz.SchedulerId = "Scheduler-Main";

            quartz.UseMicrosoftDependencyInjectionJobFactory();
            quartz.UseSimpleTypeLoader();
            quartz.UseInMemoryStore();
            quartz.UseDefaultThreadPool(tp =>
            {
                tp.MaxConcurrency = 5;
            });
                
            quartz.ScheduleJob<SimulationJob>(trigger =>
                {
                    var cronExpression = configuration["Jobs:WebhookJobCronExpression"];

                    trigger.WithIdentity(nameof(SimulationJob))
                        .StartNow()
                        .WithCronSchedule(CronScheduleBuilder.CronSchedule(cronExpression));
                }
            );
        });
            
        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        return services;
    }
}