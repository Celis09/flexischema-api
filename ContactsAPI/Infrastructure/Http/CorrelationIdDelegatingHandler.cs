namespace ContactsAPI.Infrastructure.Http
{
    /// <summary>
    /// HTTP message handler that attaches the current CorrelationId from the HttpContext 
    /// to outgoing HTTP requests via the X-Correlation-ID header.
    /// </summary>
    public class CorrelationIdDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var correlationId = _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString();

            if (!string.IsNullOrEmpty(correlationId))
            {
                request.Headers.Add("X-Correlation-Id", correlationId);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}


