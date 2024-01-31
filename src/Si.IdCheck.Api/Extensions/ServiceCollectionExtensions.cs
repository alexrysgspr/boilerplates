using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Si.IdCheck.Api.Auth;
using Si.IdCheck.Api.HealthChecks;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;
// ReSharper restore CheckNamespace

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHealthCheckServices(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<PingHealthCheck>(nameof(PingHealthCheck));

        return services;
    }

    public static IServiceCollection AddDefaultAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var identityUrl = configuration.GetValue<string>("AuthorityBaseUrl");

        services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
            .AddIdentityServerAuthentication(options =>
            {
                options.Authority = identityUrl;
                //todo ApiName
                options.ApiName = "";
                options.SupportedTokens = SupportedTokens.Jwt;
                options.RequireHttpsMetadata = true;
                options.JwtValidationClockSkew = TimeSpan.FromSeconds(30);
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthConstants.AuthPolicies.ApiClientPolicy, builder =>
            {
                builder.RequireScope(AuthConstants.AuthPolicyScopes.ApiClientPolicyScope);
            });
        });

        return services;
    }
}
