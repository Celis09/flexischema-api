using ContactsAPI.Application.AuditLogs.Dtos;
using ContactsAPI.Application.Contacts.Dtos;
using ContactsAPI.Application.Extensions;
using ContactsAPI.Application.Helper;
using ContactsAPI.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Application.AuditLogs.Queries.GetAllAuditLogs
{
    public class GetAllAuditLogsHandler(ContactsDbContext context)
        : IRequestHandler<GetAllAuditLogsQuery, PagedResult<AuditLogDto>>
    {
        public async Task<PagedResult<AuditLogDto>> Handle(GetAllAuditLogsQuery request, CancellationToken cancellationToken)
        {
            var query = context.AuditLogs
                .AsNoTracking()
                .AsQueryable()
                .ApplySearch(request.Search)
                .ApplyAuditLogFilters(request)
                .ApplySorting(request.SortBy, request.SortOrder);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(a => new AuditLogDto
                {
                    AuditLogId = a.AuditLogId,
                    Timestamp = a.Timestamp,
                    UserId = a.UserId,
                    PerformedByUsername = a.PerformedByUsername,
                    UserRole = a.UserRole,
                    ActionType = a.ActionType,
                    EntityName = a.EntityName,
                    EntityId = a.EntityId,
                    Success = a.Success,
                    ErrorMessage = a.ErrorMessage,
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<AuditLogDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
            };
        }
    }
}