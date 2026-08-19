namespace Krosoft.Extensions.Resilience.Models;

public interface ICircuitBreakerPolicyConfig
{
    int RetryCount { get; set; }
    int BreakDuration { get; set; }
}