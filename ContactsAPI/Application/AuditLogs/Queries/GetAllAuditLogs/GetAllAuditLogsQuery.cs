using ContactsAPI.Application.AuditLogs.Dtos;
using ContactsAPI.Application.Contacts.Dtos;
using MediatR;

namespace ContactsAPI.Application.AuditLogs.Queries.GetAllAuditLogs
{
    public class GetAllAuditLogsQuery : IRequest<PagedResult<AuditLogDto>>
    {
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Filters
        public string? ActionType { get; set; }
        public string? EntityName { get; set; }
        public string? UserRole { get; set; }
        public bool? Success { get; set; }
        public int? UserId { get; set; }          // optional filter
        public string? Username { get; set; }     // optional filter
    }
}
