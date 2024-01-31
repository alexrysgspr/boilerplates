using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Si.IdCheck.Workers.Application.Settings;

namespace Si.IdCheck.Workers.Application.Extensions;
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
            .Configure<ReviewMatchSettings>(configuration.GetSection(nameof(ReviewMatchSettings)));

        return services;
    }
}
