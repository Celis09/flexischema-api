using ContactsAPI.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Application.AuditLogs.Queries.GetAllAuditLogs
{
    public class GetEntityNamesHandler(ContactsDbContext context) : IRequestHandler<GetEntityNamesQuery, List<string>>
    {
        public Task<List<string>> Handle(GetEntityNamesQuery request, CancellationToken ct) =>
            context.AuditLogs
                .AsNoTracking()
                .Where(a => !string.IsNullOrEmpty(a.EntityName))
                .Select(a => a.EntityName)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync(ct);
    }
}