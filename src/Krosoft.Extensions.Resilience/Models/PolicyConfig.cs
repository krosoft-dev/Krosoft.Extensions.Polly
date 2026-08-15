namespace Krosoft.Extensions.Resilience.Models;

public record PolicyConfig : ICircuitBreakerPolicyConfig, IRetryPolicyConfig
{
    public int BreakDuration { get; set; }
    public int RetryCount { get; set; }
    public int BackoffPower { get; set; }
}