using MediatR;

namespace ContactsAPI.Application.AuditLogs.Queries.GetAllAuditLogs
{
    public record GetEntityNamesQuery() : IRequest<List<string>>;
}
