using MediatR;
using Serilog.Context;

namespace ContactsAPI.Application.Behaviors
{
    /// <summary>
    /// Intercepts MediatR requests to push the HTTP correlation ID into the Serilog LogContext.
    /// Ensures all logs emitted by the handler share the same correlation ID as the originating HTTP request.
    /// </summary>
    public class CorrelationIdBehavior<TRequest, TResponse>(IHttpContextAccessor httpContextAccessor)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var correlationId = httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString()
                ?? Guid.NewGuid().ToString();

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                return await next();
            }
        }
    }
}