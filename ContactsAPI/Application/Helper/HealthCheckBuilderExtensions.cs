using ContactsAPI.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ContactsAPI.Application.Helper
{
    public static class HealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddApiEndpointCheck(
            this IHealthChecksBuilder builder,
            string name,
            string endpoint,
            string[] tags)
        {
            return builder.AddTypeActivatedCheck<ApiEndpointHealthCheck>(
                name,
                failureStatus: HealthStatus.Unhealthy,
                tags: tags,
                args: new object[] { endpoint });
        }
    }
}

