using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ContactsAPI.Application.Abstractions
{
    public interface IHealthCheck
    {
        Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default);
    }
}
