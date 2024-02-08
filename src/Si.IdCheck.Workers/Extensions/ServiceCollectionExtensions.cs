using Microsoft.Extensions.Azure;
using Si.IdCheck.ApiClients.CloudCheck.Extensions;
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
            .AddScoped<IOngoingMonitoringAlertsService, OngoingMonitoringAlertsService>()
            .AddCloudCheck(configuration)
            .AddApplicationDependencies(configuration)
            .AddHostedService<AlertsWorker>()
            .AddHostedService<JobsWorker>()
            .AddConfigurations(configuration)
            .AddHealthChecks()
            .AddCheck<PingHealthCheck>(nameof(PingHealthCheck));

        services
            .AddAzureClients(builder =>
            {
                builder
                    .AddServiceBusClient(configuration.GetConnectionString("Servicebus"))
                    .WithName("SwiftId")
                    .ConfigureOptions(options =>
                    {
                        options.RetryOptions.Delay = TimeSpan.FromMilliseconds(50);
                        options.RetryOptions.MaxDelay = TimeSpan.FromSeconds(5);
                        options.RetryOptions.MaxRetries = 3;
                    });
            });

        return services;
    }

    public static IServiceCollection AddConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .Configure<AlertsWorkerSettings>(configuration.GetSection(nameof(AlertsWorkerSettings)));

        return services;
    }
}
