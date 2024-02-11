using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using Serilog.Events;
using Si.IdCheck.Workers.Settings;
using ILogger = Serilog.ILogger;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.Hosting;
// ReSharper restore CheckNamespace

public static class HostExtensions
{
    public static ILogger CreateDefaultLogger(this IHost host)
    {
        var configuration = host.Services.GetRequiredService<IConfiguration>();

        var serilogConfigSection = configuration.GetSection("Serilog");
        var minimumLevelSection = serilogConfigSection?.GetSection("MinimumLevel");
        var minimumLevelDefaultSection = minimumLevelSection?.GetSection("Default");

        if (minimumLevelDefaultSection is null)
        {
            throw new Exception("Serilog configuration is invalid.");
        }

        if (!Enum.TryParse(typeof(LogEventLevel), minimumLevelDefaultSection.Value, out var logEVentLevel))
        {
            throw new Exception("Serilog configuration is invalid. LogEventLevel value for MinimumLevel.Default cannot be parsed.");
        }

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Is((LogEventLevel)logEVentLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(LogEventLevel.Information);

        var logSettings = configuration.GetSection(nameof(LogSettings)).Get<LogSettings>();
        var instrumentationKey = configuration.GetValue<string>("ApplicationInsights:InstrumentationKey");
        if (!string.IsNullOrWhiteSpace(logSettings.LogFilePath))
        {
            loggerConfiguration = loggerConfiguration.WriteTo.File(logSettings.LogFilePath, logSettings.LogEventLevel);
        }

        if (!string.IsNullOrWhiteSpace(instrumentationKey))
        {
            loggerConfiguration = loggerConfiguration.WriteTo.ApplicationInsights(
                new TelemetryConfiguration(instrumentationKey),
                TelemetryConverter.Traces,
                logSettings.LogEventLevel);
        }

        return loggerConfiguration.CreateBootstrapLogger();
    }
}
