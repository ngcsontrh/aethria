using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Aethria.Api.Extensions;

/// <summary>
/// Extension methods for configuring dynamic CORS services.
/// </summary>
public static class CorsServiceExtensions
{
    /// <summary>
    /// Adds dynamic configuration-based CORS policies to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddConfigurableCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors();
        services.Replace(ServiceDescriptor.Singleton<ICorsPolicyProvider, ConfigurableCorsPolicyProvider>());
        return services;
    }
}

internal class ConfigurableCorsPolicyProvider : ICorsPolicyProvider
{
    private readonly List<CorsPolicySetting> _policies;
    private readonly CorsOptions _options;

    public ConfigurableCorsPolicyProvider(IOptions<CorsOptions> options, IConfiguration configuration)
    {
        _policies = configuration.GetSection("Cors").Get<List<CorsPolicySetting>>() ?? new();
        _options = options.Value;
    }

    public Task<CorsPolicy?> GetPolicyAsync(HttpContext context, string? policyName)
    {
        if (!string.IsNullOrEmpty(policyName))
        {
            var policy = _options.GetPolicy(policyName);
            if (policy != null)
            {
                return Task.FromResult<CorsPolicy?>(policy);
            }
        }

        var origin = context.Request.Headers.Origin.ToString();
        if (string.IsNullOrEmpty(origin))
        {
            return Task.FromResult<CorsPolicy?>(_options.GetPolicy(_options.DefaultPolicyName));
        }

        var matchingPolicy = _policies.FirstOrDefault(p =>
            p.Origins != null && p.Origins.Contains(origin, StringComparer.OrdinalIgnoreCase));

        if (matchingPolicy == null)
        {
            return Task.FromResult<CorsPolicy?>(_options.GetPolicy(_options.DefaultPolicyName));
        }

        var corsPolicyBuilder = new CorsPolicyBuilder()
            .WithOrigins(origin);

        if (matchingPolicy.AllowedMethods != null && matchingPolicy.AllowedMethods.Any())
        {
            if (matchingPolicy.AllowedMethods.Contains("*"))
            {
                corsPolicyBuilder.AllowAnyMethod();
            }
            else
            {
                corsPolicyBuilder.WithMethods(matchingPolicy.AllowedMethods);
            }
        }

        if (matchingPolicy.AllowedHeaders != null && matchingPolicy.AllowedHeaders.Any())
        {
            if (matchingPolicy.AllowedHeaders.Contains("*"))
            {
                corsPolicyBuilder.AllowAnyHeader();
            }
            else
            {
                corsPolicyBuilder.WithHeaders(matchingPolicy.AllowedHeaders);
            }
        }

        if (matchingPolicy.AllowCredentials)
        {
            corsPolicyBuilder.AllowCredentials();
        }
        else
        {
            corsPolicyBuilder.DisallowCredentials();
        }

        return Task.FromResult<CorsPolicy?>(corsPolicyBuilder.Build());
    }
}

internal class CorsPolicySetting
{
    public string[] Origins { get; set; } = Array.Empty<string>();
    public string[] AllowedMethods { get; set; } = Array.Empty<string>();
    public string[] AllowedHeaders { get; set; } = Array.Empty<string>();
    public bool AllowCredentials { get; set; }
}
