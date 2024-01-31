using Si.IdCheck.Workers.HealthChecks;
using Si.IdCheck.Workers.Jobs;
using Si.IdCheck.Workers.Services;
using Si.IdCheck.Workers.Settings;

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
            .AddMediatR(mediatorConfig =>
            {
                //todo
                mediatorConfig.RegisterServicesFromAssembly(typeof(Program).Assembly);
            })
            .AddServiceBus(configuration)
            .AddVerifidentity(configuration)
            //.AddHostedService<JobsWorker>()
            .AddHostedService<AlertsWorker>()
            .AddSingleton<IAlertsWorkerService, AlertsWorkerService>()
            .AddConfigurations(configuration)
            .AddHealthChecks()
            .AddCheck<PingHealthCheck>(nameof(PingHealthCheck));

        return services;
    }

    public static IServiceCollection AddConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .Configure<AlertsWorkerSettings>(configuration.GetSection(nameof(AlertsWorkerSettings)));

        return services;
    }
}
