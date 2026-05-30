using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ContactsAPI.Services
{
    public class ApiEndpointHealthCheck : IHealthCheck
    {
        private readonly HttpClient _httpClient;
        private readonly string _endpoint;

        public ApiEndpointHealthCheck(HttpClient httpClient, string endpoint)
        {
            _httpClient = httpClient;
            _endpoint = endpoint;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync(_endpoint, cancellationToken);
                if (response.IsSuccessStatusCode)
                    return HealthCheckResult.Healthy($"{_endpoint} is reachable.");
                return HealthCheckResult.Unhealthy($"{_endpoint} returned {response.StatusCode}.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"{_endpoint} unreachable.", ex);
            }
        }
    }
}


