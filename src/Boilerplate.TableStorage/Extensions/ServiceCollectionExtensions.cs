using Azure.Data.Tables;
using Boilerplate.TableStorage;
using Boilerplate.TableStorage.Models;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;
// ReSharper restore CheckNamespace

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTableStorageServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAzureClients(builder =>
            {
                builder
                    .AddTableServiceClient(configuration.GetConnectionString(TableStorageConsts.ConnectionStringName));
            });

        services
            .AddSingleton<IAzureTableStorageService<AzureTableStorageEntity>>(provider =>
            new AzureTableStorageService<AzureTableStorageEntity>(provider.GetRequiredService<TableServiceClient>(), AzureTableStorageEntityConsts.TableName));

        return services;
    }
}