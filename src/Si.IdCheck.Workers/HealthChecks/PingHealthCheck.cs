using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Si.IdCheck.Workers.HealthChecks;

public class PingHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.FromResult(HealthCheckResult.Healthy());
    }
}