using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Si.Onboarding.ServiceBus;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;
// ReSharper restore CheckNamespace

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceBus(this IServiceCollection services, IConfiguration configuration)
    {

        var connectionString = configuration.GetConnectionString("ServiceBus");

        services
            .AddSingleton(o => new ServiceBusClient(connectionString))
            .AddSingleton<IServiceBusFactory, ServiceBusFactory>();

        return services;
    }
}
