using Serilog.Events;

namespace Boilerplate.Workers.Settings;

public class LogSettings
{
    public string LogFilePath { get; set; }

    public LogEventLevel LogEventLevel { get; set; }
}