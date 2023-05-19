using Serilog.Events;

namespace Banking.Simulation.Core.Options;

public sealed class LoggingOptions
{
    public LogEventLevel ConsoleLogLevel { get; init; }
}