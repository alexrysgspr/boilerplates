using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Boilerplate.Api.HealthChecks;

public class PingHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.FromResult(HealthCheckResult.Healthy());
    }
}