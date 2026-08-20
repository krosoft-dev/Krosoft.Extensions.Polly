namespace Krosoft.Extensions.Resilience.Models;

public interface IRetryPolicyConfig
{
    int RetryCount { get; set; }
    int BackoffPower { get; set; }
}