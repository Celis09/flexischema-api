using MediatR;

namespace ContactsAPI.Application.Admins.Commands.ImportContacts
{
    public record ImportContactsCommand(
        string CsvContent,
        bool AutoCreateDefinitions = false,
        bool OverwriteExisting = false,
        bool DryRun = false
    ) : IRequest<ImportResult>;
}
