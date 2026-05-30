using ContactsAPI.Application.AuditLogs.Dtos;
using ContactsAPI.Application.Contacts.Dtos;
using ContactsAPI.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Application.AuditLogs.Queries.GetAdminActionsSummary
{
    public class GetAdminActionsSummaryHandler(ContactsDbContext context)
        : IRequestHandler<GetAdminActionsSummaryQuery, PagedResult<AdminActionSummaryDto>>
    {
        public async Task<PagedResult<AdminActionSummaryDto>> Handle(
            GetAdminActionsSummaryQuery request,
            CancellationToken cancellationToken)
        {
            var filter = request.Filter;
            var query = context.AuditLogs.AsQueryable();

            DateTime fromDate;
            if (filter.FromDate.HasValue)
            {
                fromDate = filter.FromDate.Value.Date;
            }
            else if (filter.UseCalendarDay)
            {
                fromDate = DateTime.UtcNow.Date;
            }
            else
            {
                fromDate = DateTime.UtcNow.AddHours(-24);
            }

            query = query.Where(a => a.Timestamp >= fromDate);

            if (filter.ToDate.HasValue)
            {
                var endOfDay = filter.ToDate.Value.Date.AddDays(1);
                query = query.Where(a => a.Timestamp < endOfDay);
            }

            if (!string.IsNullOrEmpty(filter.Role))
                query = query.Where(a => a.UserRole == filter.Role);

            var grouped = query
                .GroupBy(a => a.ActionType)
                .Select(g => new AdminActionSummaryDto
                {
                    ActionType = g.Key,
                    Count = g.Count()
                });

            grouped = string.Equals(filter.SortBy, "actiontype", StringComparison.OrdinalIgnoreCase) switch
            {
                true => string.Equals(filter.SortOrder, "asc", StringComparison.OrdinalIgnoreCase)
                    ? grouped.OrderBy(x => x.ActionType)
                    : grouped.OrderByDescending(x => x.ActionType),
                false => string.Equals(filter.SortOrder, "asc", StringComparison.OrdinalIgnoreCase)
                    ? grouped.OrderBy(x => x.Count)
                    : grouped.OrderByDescending(x => x.Count)
            };

            var totalCount = await grouped.CountAsync(cancellationToken);

            var items = await grouped
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<AdminActionSummaryDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }
    }
}