using MediatR;

namespace ContactsAPI.Application.Behaviors
{
    /// <summary>
    /// Global exception handler for the MediatR pipeline. 
    /// Catches unhandled exceptions, logs them with the request name, and rethrows them for the middleware to format.
    /// </summary>
    public class ExceptionHandlingBehavior<TRequest, TResponse>(ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
        : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception for request {RequestName}", typeof(TRequest).Name);
                throw;
            }
        }
    }
}