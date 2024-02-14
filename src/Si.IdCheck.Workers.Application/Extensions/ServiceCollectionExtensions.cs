using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Si.IdCheck.Workers.Application;
using Si.IdCheck.Workers.Application.Reviewers;
using Si.IdCheck.Workers.Application.Reviewers.Omg;
using Si.IdCheck.Workers.Application.ServiceBus;
using Si.IdCheck.Workers.Application.Settings;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;
// ReSharper restore CheckNamespace
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddMediatR(mediatorConfig =>
            {
                mediatorConfig.RegisterServicesFromAssembly(typeof(WorkersApplication).Assembly);
            });

        services
            .AddTableStorageServices(configuration)
            .AddReviewerDependencies(configuration);

        services
            .Configure<GetAssociationsSettings>(configuration.GetSection(nameof(GetAssociationsSettings)))
            .Configure<ReviewerSettings>("omg", configuration.GetSection(nameof(OmgReviewerSettings)));

        services
            .AddAzureClients(builder =>
            {
                services
                    .Configure<ServiceBusSettings>(configuration.GetSection(nameof(ServiceBusSettings)));

                builder
                    .AddServiceBusClient(configuration.GetConnectionString(ServiceBusConsts.ConnectionStringName))
                    .WithName(ServiceBusConsts.ClientName)
                    .ConfigureOptions(options =>
                    {
                        options.RetryOptions.Delay = TimeSpan.FromMilliseconds(50);
                        options.RetryOptions.MaxDelay = TimeSpan.FromSeconds(5);
                        options.RetryOptions.MaxRetries = 1;
                    });
            });

        return services;
    }


    public static IServiceCollection AddReviewerDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddTransient<OmgReviewer>();

        services.AddTransient<Func<string, IReviewer>>(serviceProvider => (key) =>
        {
            switch (key)
            {
                case "omg":
                    return serviceProvider.GetRequiredService<OmgReviewer>();
                default:
                    throw new KeyNotFoundException($"Invalid key {key}.");
            }
        });

        return services;
    }
}
