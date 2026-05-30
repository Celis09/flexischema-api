using MediatR;

namespace ContactsAPI.Application.AuditLogs.Queries.GetAllAuditLogs
{
    public record GetActionTypesQuery() : IRequest<List<string>>;
}
