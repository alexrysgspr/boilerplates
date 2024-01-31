using Serilog.Events;

namespace Si.IdCheck.Api.Settings;

public class LogSettings
{
    public string LogFilePath { get; set; }

    public LogEventLevel LogEventLevel { get; set; }
}