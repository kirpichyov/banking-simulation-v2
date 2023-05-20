using System;
using Microsoft.Extensions.Logging;
using Quartz.Logging;
using LogLevel = Quartz.Logging.LogLevel;
// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace Banking.Simulation.Application.Quartz;

internal sealed class QuartzLogProvider : ILogProvider
{
    private readonly ILogger<QuartzLogProvider> _logger;

    public QuartzLogProvider(ILogger<QuartzLogProvider> logger)
    {
        _logger = logger;
    }

    public Logger GetLogger(string name)
    {
        return (level, func, exception, parameters) =>
        {
            if (func is null)
            {
                return true;
            }
                
            var log = func();
                    
            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Trace:
                    _logger.LogDebug(log, parameters);
                    return true;
                case LogLevel.Info:
                    _logger.LogInformation(log, parameters);
                    break;
                case LogLevel.Warn:
                    _logger.LogWarning(log, parameters);
                    break;
                case LogLevel.Error:
                    _logger.LogError(exception, log, parameters);
                    break;
                case LogLevel.Fatal:
                    _logger.LogCritical(exception, log, parameters);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }

            return true;
        };
    }

    public IDisposable OpenNestedContext(string message)
    {
        throw new NotImplementedException();
    }

    public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
    {
        throw new NotImplementedException();
    }
}