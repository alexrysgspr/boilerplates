using Boilerplate.Workers.HealthChecks;
using Boilerplate.Workers.Jobs;
using Boilerplate.Workers.Services;
using Boilerplate.Workers.Settings;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;
// ReSharper restore CheckNamespace

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions()
            .AddSingleton<IDateTimeService, DateTimeService>()
            .AddBoilerplateApiClient(configuration)
            .AddApplicationDependencies(configuration)
            .AddHostedService<JobsWorker>()
            .AddConfigurations(configuration)
            .AddHealthChecks()
            .AddCheck<PingHealthCheck>(nameof(PingHealthCheck));

        return services;
    }

    public static IServiceCollection AddConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .Configure<OngoingMonitoringAlertsWorkerSettings>(configuration.GetSection(nameof(OngoingMonitoringAlertsWorkerSettings)));

        return services;
    }
}
