using ContactsAPI.Application.AuditLogs.Dtos;
using ContactsAPI.Application.Contacts.Dtos;
using MediatR;

namespace ContactsAPI.Application.AuditLogs.Queries.GetAdminActionsSummary
{
    public record GetAdminActionsSummaryQuery(AdminActionSummaryFilter Filter)
    : IRequest<PagedResult<AdminActionSummaryDto>>;
}
