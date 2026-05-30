using ContactsAPI.Application.Abstractions;
using ContactsAPI.Application.Attributes;
using ContactsAPI.Application.Exceptions;
using MediatR;

namespace ContactsAPI.Application.Behaviors
{
    /// <summary>
    /// Enforces role-based authorization rules defined via <see cref="AuthorizeRoleAttribute"/> on MediatR requests.
    /// Throws an UnauthorizedAccessAppException if the current user does not meet the role requirements.
    /// </summary>
    public class RoleAuthorizationBehavior<TRequest, TResponse>(IUserContext userContext)
        : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (typeof(TRequest)
                .GetCustomAttributes(typeof(AuthorizeRoleAttribute), true)
                .FirstOrDefault() is AuthorizeRoleAttribute authorizeAttr)
            {
                var role = userContext?.Role;
                if (string.IsNullOrEmpty(role) || !authorizeAttr.Roles.Contains(role))
                {
                    throw new UnauthorizedAccessAppException(
                        $"Role '{role ?? "none"}' is not authorized to execute {typeof(TRequest).Name}");
                }
            }

            return await next();
        }
    }
}