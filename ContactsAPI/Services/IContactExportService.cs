using ContactsAPI.Application.Contacts.Queries.ExportContacts;
using ContactsAPI.Entities;

namespace ContactsAPI.Services
{
    public interface IContactExportService
    {
        ExportResult GenerateExport(
            ExportContactsQuery request,
            List<Contact> contacts,
            List<ExtraFieldDefinition> definitions,
            bool isSelective);
    }
}