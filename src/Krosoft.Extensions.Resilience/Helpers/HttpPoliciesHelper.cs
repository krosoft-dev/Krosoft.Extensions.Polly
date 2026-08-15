using System.Security.Cryptography;
using Krosoft.Extensions.Resilience.Models;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;

namespace Krosoft.Extensions.Resilience.Helpers;

public static class HttpPoliciesHelper
{
    public static PolicyBuilder<HttpResponseMessage> GetBaseBuilder() => HttpPolicyExtensions.HandleTransientHttpError();

    public static IAsyncPolicy<HttpResponseMessage> GetHttpCircuitBreakerPolicy(ILogger logger, ICircuitBreakerPolicyConfig circuitBreakerPolicyConfig)
    {
        return GetBaseBuilder()
            .CircuitBreakerAsync(circuitBreakerPolicyConfig.RetryCount,
                                 TimeSpan.FromSeconds(circuitBreakerPolicyConfig.BreakDuration),
                                 (result, breakDuration) =>

                                 {
                                     if (result == null)
                                     {
                                         logger.LogWarning("Service shutdown during {BreakDuration} after {RetryCount} failed retries.",
                                                           breakDuration, circuitBreakerPolicyConfig.RetryCount);

                                         throw new BrokenCircuitException("Service inoperative. Please try again later...");
                                     }

                                     if (result.Exception != null)
                                     {
                                         logger.LogWarning("Service shutdown during {BreakDuration} after {RetryCount} failed retries : {StatusCode} {Message}",
                                                           breakDuration, circuitBreakerPolicyConfig.RetryCount, result.Result?.StatusCode, result.Exception.Message);

                                         throw new BrokenCircuitException($"Service inoperative. Please try again later : {result.Result?.StatusCode} {result.Exception.Message}", result.Exception);
                                     }

                                     if (result.Result != null)
                                     {
                                         var message = result.Result.Content.ReadAsStringAsync().Result;

                                         logger.LogWarning("Service shutdown during {BreakDuration} after {RetryCount} failed retries : {StatusCode} {Message}",
                                                           breakDuration, circuitBreakerPolicyConfig.RetryCount, result.Result.StatusCode, message);

                                         throw new BrokenCircuitException($"Service inoperative. Please try again later : {result.Result.StatusCode} {message}");
                                     }
                                 },
                                 () => { logger.LogInformation("Service restarted."); });
    }

    public static IAsyncPolicy<HttpResponseMessage> GetHttpRetryPolicy(ILogger logger, IRetryPolicyConfig retryPolicyConfig)
    {
        return GetBaseBuilder()
            .WaitAndRetryAsync(retryPolicyConfig.RetryCount,
                               retryAttempt => TimeSpan.FromSeconds(Math.Pow(retryPolicyConfig.BackoffPower, retryAttempt)) + TimeSpan.FromMilliseconds(RandomNumberGenerator.GetInt32(0, 100)),
                               (result, timeSpan, retryCount, _) =>
                               {
                                   if (result.Result != null)
                                   {
                                       logger.LogWarning("Request failed with {StatusCode}. Waiting {Delay} before next retry. Retry attempt {RetryCount}",
                                                         result.Result.StatusCode, timeSpan, retryCount);
                                   }
                                   else
                                   {
                                       logger.LogWarning("Request failed because network failure. Waiting {Delay} before next retry. Retry attempt {RetryCount}",
                                                         timeSpan, retryCount);
                                   }
                               });
    }
}