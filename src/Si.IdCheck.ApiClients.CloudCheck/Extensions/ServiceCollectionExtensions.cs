using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Polly;
using Si.IdCheck.ApiClients.Verifidentity;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;
// ReSharper restore CheckNamespace

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVerifidentity(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(nameof(VerifidentitySettings));
        services.Configure<VerifidentitySettings>(settings);

        services
            .AddHttpClient<IVerifidentityApiClient, VerifidentityApiClient>(client =>
        {
            client.BaseAddress = new Uri(settings.Get<VerifidentitySettings>().BaseUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        })
        .AddTransientHttpErrorPolicy(builder =>
            builder
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(15)
        })
        .SetHandlerLifetime(Timeout.InfiniteTimeSpan);

        return services;
    }
}
