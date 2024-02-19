using Serilog.Events;

namespace Boilerplate.Api.Settings;

public class LogSettings
{
    public string LogFilePath { get; set; }

    public LogEventLevel LogEventLevel { get; set; }
}