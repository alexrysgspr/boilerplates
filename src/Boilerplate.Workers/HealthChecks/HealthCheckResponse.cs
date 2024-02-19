namespace Boilerplate.Workers.HealthChecks;

public class HealthCheckResponse
{
    public string Status { get; set; }
    public HealthCheckEntry[] HealthCheckEntries { get; set; }
}

public class HealthCheckEntry
{
    public string Name { get; set; }
    public string Status { get; set; }
    public object Errors { get; set; }
}