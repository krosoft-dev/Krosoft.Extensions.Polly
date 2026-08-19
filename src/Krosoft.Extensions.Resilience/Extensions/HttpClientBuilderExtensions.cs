using Krosoft.Extensions.Resilience.Helpers;
using Krosoft.Extensions.Resilience.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Krosoft.Extensions.Resilience.Extensions;

public static class HttpClientBuilderExtensions
{
    private static readonly Func<IServiceCollection, ILogger<IHttpClientBuilder>> LoggerFunc = serviceCollection =>
    {
        var sp = serviceCollection.BuildServiceProvider();

        var logger = sp.GetRequiredService<ILogger<IHttpClientBuilder>>();

        return logger;
    };

    public static IHttpClientBuilder AddCircuitBreakerHandler(this IHttpClientBuilder httpClientBuilder,
                                                              IServiceCollection serviceCollection,
                                                              ICircuitBreakerPolicyConfig circuitBreakerPolicyConfig) =>
        httpClientBuilder.AddPolicyHandler(HttpPoliciesHelper.GetHttpCircuitBreakerPolicy(LoggerFunc(serviceCollection), circuitBreakerPolicyConfig));

    public static IHttpClientBuilder AddPolicyHandlers(this IHttpClientBuilder httpClientBuilder,
                                                       IConfiguration configuration,
                                                       IServiceCollection serviceCollection)
    {
        var policyConfig = new PolicyConfig();
        configuration.Bind(nameof(PolicyConfig), policyConfig);

        var circuitBreakerPolicyConfig = (ICircuitBreakerPolicyConfig)policyConfig;
        var retryPolicyConfig = (IRetryPolicyConfig)policyConfig;

        httpClientBuilder.SetHandlerLifetime(TimeSpan.FromMinutes(5)) 
                         .AddRetryPolicyHandler(serviceCollection, retryPolicyConfig)
                         .AddCircuitBreakerHandler(serviceCollection, circuitBreakerPolicyConfig)
            ;

        return httpClientBuilder;
    }

    public static IHttpClientBuilder AddRetryPolicyHandler(this IHttpClientBuilder httpClientBuilder, IServiceCollection serviceCollection, IRetryPolicyConfig retryPolicyConfig) =>
        httpClientBuilder.AddPolicyHandler(HttpPoliciesHelper.GetHttpRetryPolicy(LoggerFunc(serviceCollection), retryPolicyConfig));
}