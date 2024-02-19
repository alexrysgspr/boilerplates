using System.Net.Http.Headers;
using Boilerplate.ApiClients.Boilerplate;
using Microsoft.Extensions.Configuration;
using Polly;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;
// ReSharper restore CheckNamespace
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBoilerplateApiClient(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(nameof(BoilerplateApiClientSettings));
        services.Configure<BoilerplateApiClientSettings>(settings);

        services
            .AddHttpClient<IBoilerplateApiClient, BoilerplateApiClient>(client =>
            {
                client.BaseAddress = new Uri(settings.Get<BoilerplateApiClientSettings>().BaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            })
        .AddTransientHttpErrorPolicy(builder =>
            builder
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

        return services;
    }
}
