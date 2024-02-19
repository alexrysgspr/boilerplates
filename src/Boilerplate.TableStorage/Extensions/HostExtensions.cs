using Boilerplate.TableStorage;
using Boilerplate.TableStorage.Models;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.Hosting;
// ReSharper restore CheckNamespace

public static class HostExtensions
{
    public static async Task SetupTableStorageServices(this IHost host)
    {
        await host?
            .Services?
            .GetService<IAzureTableStorageService<AzureTableStorageEntity>>()?
            .SetupAsync()!;
    }
}
