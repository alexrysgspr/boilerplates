using Microsoft.Extensions.Configuration;
using Si.IdCheck.Workers.Application;
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
            .AddTableStorageServices(configuration);

        services
            .Configure<ReviewMatchSettings>(configuration.GetSection(nameof(ReviewMatchSettings)))
            .Configure<GetAssociationsSettings>(configuration.GetSection(nameof(GetAssociationsSettings)));

        return services;
    }
}
