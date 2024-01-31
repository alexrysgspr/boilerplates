using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Serilog;
using Si.IdCheck.Workers.Settings;

// ReSharper disable CheckNamespace
namespace Microsoft.AspNetCore.Builder;
// ReSharper restore CheckNamespace
public static class ProgramExtensions
{
    public static IHostBuilder ConfigureDefaultAppConfiguration<T>(this IHostBuilder hostBuilder) where T : class
    {
        hostBuilder.ConfigureAppConfiguration(
            (context, config) =>
            {
                config.AddDefaultSources<T>(context);
            })
            .UseSerilog();

        return hostBuilder;
    }

    public static IConfigurationBuilder AddDefaultSources<T>(
        this IConfigurationBuilder configBuilder,
        HostBuilderContext builderContext) where T : class
    {
        IHostEnvironment env = builderContext.HostingEnvironment;

        configBuilder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        if (builderContext.HostingEnvironment.IsDevelopment())
        {
            configBuilder.AddUserSecrets<T>();
        }


        AddDefaultAzureKeyVault(configBuilder);


        return configBuilder;
    }

    private static void AddDefaultAzureKeyVault(IConfigurationBuilder builder)
    {
        var builtConfig = builder.Build();
        var appSettings = new AppSettings();
        builtConfig.GetSection(nameof(AppSettings)).Bind(appSettings);

        if (string.IsNullOrWhiteSpace(appSettings.KeyVaultUri))
        {
            return;
        }

        var azureServiceTokenProvider = new AzureServiceTokenProvider();
        var keyVaultClient = new KeyVaultClient(
            new KeyVaultClient.AuthenticationCallback(
                azureServiceTokenProvider.KeyVaultTokenCallback));
        builder.AddAzureKeyVault(
            appSettings.KeyVaultUri, keyVaultClient, new DefaultKeyVaultSecretManager());
    }
}
