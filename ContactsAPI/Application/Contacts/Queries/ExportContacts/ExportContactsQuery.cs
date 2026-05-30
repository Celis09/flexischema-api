using MediatR;

namespace ContactsAPI.Application.Contacts.Queries.ExportContacts
{
    public record ExportContactsQuery(
    string Format,
    string? Columns = null,
    string? Ids = null,
    bool IsAdmin = false,
    bool IsEditor = false
) : IRequest<ExportResult>;
}
