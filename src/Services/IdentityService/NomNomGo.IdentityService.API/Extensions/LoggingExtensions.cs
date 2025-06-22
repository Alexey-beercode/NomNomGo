using Serilog;
using Serilog.Events;

namespace NomNomGo.IdentityService.API.Extensions;

/// <summary>
/// Logging configuration extension
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Configures default logging if no logging configuration is found in settings
    /// </summary>
    /// <param name="configuration">The logger configuration to extend</param>
    /// <param name="environment">The current hosting environment</param>
    /// <returns>The logger configuration with default settings applied</returns>
    public static LoggerConfiguration ConfigureDefaultLogging(
        this LoggerConfiguration configuration, 
        IHostEnvironment environment)
    {
        if (configuration.WriteTo is null)
        {
            var logLevel = environment.IsDevelopment() 
                ? LogEventLevel.Debug 
                : LogEventLevel.Information;
                
            configuration
                .MinimumLevel.Is(logLevel)
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: $"logs/webparserservice-{environment.EnvironmentName}-.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
        }
            
        return configuration;
    }
}