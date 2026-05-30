using Serilog.Context;

namespace ContactsAPI.Middleware
{
    /// <summary>
    /// Assigns a unique Correlation ID to every incoming HTTP request.
    /// If the client provides an X-Correlation-Id header it is reused; otherwise a new GUID is generated.
    /// The ID is stored in HttpContext.Items and pushed into Serilog's LogContext for end-to-end tracing.
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                                ?? Guid.NewGuid().ToString();

            // Store in Items for downstream behaviors
            context.Items["CorrelationId"] = correlationId;

            // Safely set response header (overwrite if exists)
            context.Response.Headers["X-Correlation-Id"] = correlationId;

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }
    }
}
