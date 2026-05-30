using ContactsAPI.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Application.AuditLogs.Queries.GetAllAuditLogs
{
    public class GetActionTypesHandler(ContactsDbContext context) : IRequestHandler<GetActionTypesQuery, List<string>>
    {
        public Task<List<string>> Handle(GetActionTypesQuery request, CancellationToken ct) =>
            context.AuditLogs
                .AsNoTracking()
                .Where(a => !string.IsNullOrEmpty(a.ActionType))
                .Select(a => a.ActionType)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync(ct);
    }
}