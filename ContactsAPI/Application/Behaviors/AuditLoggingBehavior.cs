using ContactsAPI.Application.Contacts.Commands.CreateContact;
using ContactsAPI.Application.Helper;
using ContactsAPI.Data;
using ContactsAPI.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;

namespace ContactsAPI.Application.Behaviors
{
    /// <summary>
    /// Intercepts MediatR requests to capture an audit log of commands (mutations).
    /// Uses an isolated DbContext factory to ensure the audit log is saved independently of the main request transaction.
    /// </summary>
    public class AuditLoggingBehavior<TRequest, TResponse>(
        IHttpContextAccessor httpContextAccessor,
        IDbContextFactory<ContactsDbContext> dbFactory,
        IMemoryCache cache,
        ILogger<AuditLoggingBehavior<TRequest, TResponse>> logger)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var enableAudit = await cache.GetOrCreateAsync("audit:enabled", async entry =>
            {
                await using var context = await dbFactory.CreateDbContextAsync(cancellationToken);
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return await context.AdminConfigs
                    .Where(c => c.Key == "EnableAuditLogging")
                    .Select(c => c.Value)
                    .FirstOrDefaultAsync(cancellationToken);
            });

            if (!bool.TryParse(enableAudit, out var auditEnabled) || !auditEnabled)
            {
                logger.LogDebug("AuditLoggingBehavior: audit disabled by AdminConfig.");
                return await next();
            }

            var httpContext = httpContextAccessor.HttpContext;

            var userIdClaim = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? httpContext?.User?.FindFirst("sub")?.Value;
            int? userId = int.TryParse(userIdClaim, out var parsedId) ? parsedId : null;

            // ADDED: read username from ClaimTypes.Name (set in AuthController login claims)
            var usernameClaim = httpContext?.User?.FindFirst(ClaimTypes.Name)?.Value
                ?? "Anonymous";

            var roleClaim = httpContext?.User?.FindFirst("role")?.Value
                ?? httpContext?.User?.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value
                ?? "Viewer";

            var requestName = typeof(TRequest).Name;

            logger.LogDebug("AuditLoggingBehavior: handling request {RequestType} by userId={UserId} username={Username} role={Role}",
                requestName, userId?.ToString() ?? "anonymous", usernameClaim, roleClaim);

            if (request is CreateContactCommand createCmd)
            {
                var count = createCmd.ExtraFields?.Count ?? 0;
                var elementType = createCmd.ExtraFields?.FirstOrDefault()?.GetType().FullName ?? "none";
                logger.LogDebug("CreateContactCommand ExtraFields count={Count} elementType={ElementType}", count, elementType);
            }

            var auditLog = new AuditLog
            {
                Timestamp = PhilippineTime.Now,
                UserId = userId,
                PerformedByUsername = usernameClaim,   // ← ADDED: now populated from JWT claim
                UserRole = roleClaim,
                ActionType = requestName,
                EntityName = ExtractEntityName(requestName),
                RequestData = System.Text.Json.JsonSerializer.Serialize(request),
            };

            try
            {
                var response = await next();

                auditLog.ResponseData = System.Text.Json.JsonSerializer.Serialize(response);
                auditLog.Success = true;
                auditLog.ErrorMessage = string.Empty;
                auditLog.EntityId = response?.ToString() ?? "N/A";

                return response;
            }
            catch (Exception ex)
            {
                auditLog.Success = false;
                auditLog.ErrorMessage = ex.Message;
                logger.LogError(ex, "AuditLoggingBehavior: exception while handling {RequestType}", requestName);
                throw;
            }
            finally
            {
                try
                {
                    await using var context = await dbFactory.CreateDbContextAsync(cancellationToken);
                    context.AuditLogs.Add(auditLog);
                    await context.SaveChangesAsync(cancellationToken);
                    logger.LogDebug("AuditLoggingBehavior: audit log saved for {RequestType}", requestName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "AuditLoggingBehavior: failed to persist audit log for {RequestType}", requestName);
                }
            }
        }

        private static string ExtractEntityName(string requestName)
        {
            var name = requestName
                .Replace("Command", "")
                .Replace("Query", "");

            string[] verbs = ["Create", "Update", "Delete", "GetAll", "Get",
                              "Add", "Remove", "Change", "Assign",
                              "Activate", "Deactivate", "Status"];

            foreach (var verb in verbs)
                name = name.Replace(verb, "");

            return name.Trim();
        }
    }
}