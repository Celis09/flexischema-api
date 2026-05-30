using FluentValidation;
using FluentValidation.Results;
using MediatR;
using AppValidationException = ContactsAPI.Application.Exceptions.ValidationException;

namespace ContactsAPI.Application.Behaviors
{
    /// <summary>
    /// Intercepts MediatR requests to run FluentValidation rules before the handler executes.
    /// Runs all validators in parallel and throws a ValidationException if any failures occur.
    /// </summary>
    public class ValidationBehavior<TRequest, TResponse>(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var validatorsList = validators?.ToList() ?? [];
            logger.LogDebug("ValidationBehavior: {Count} validators for {RequestType}",
                validatorsList.Count, typeof(TRequest).FullName);

            if (validatorsList.Count > 0)
            {
                var context = new ValidationContext<TRequest>(request);
                
                var results = await Task.WhenAll(
                    validatorsList.Select(v => v.ValidateAsync(context, cancellationToken)));
                
                var failures = results
                    .SelectMany(r => r.Errors)
                    .Where(f => f != null)
                    .ToList();

                logger.LogDebug("ValidationBehavior: {Failures} validation failures for {RequestType}",
                    failures.Count, typeof(TRequest).FullName);

                if (failures.Count != 0)
                {
                    var errors = failures
                        .GroupBy(f => string.IsNullOrWhiteSpace(f.PropertyName) ? "_" : f.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(f => f.ErrorMessage).ToArray()
                        );
                    throw new AppValidationException(errors);
                }
            }

            return await next();
        }
    }
}