using ContactsAPI.Entities;

namespace ContactsAPI.Application.Admins.Commands.ImportContacts
{
    public class ImportTracker
    {
        public int ImportedCount { get; private set; }
        public int UpdatedCount { get; private set; }
        public int SkippedCount { get; private set; }
        public List<int> FailedRows { get; } = new();
        public List<string> Errors { get; } = new();
        public List<ImportRowPreview> RowPreviews { get; } = new();
        public List<Contact> NewContacts { get; } = new();
        public List<ContactExtraField> NewExtraFields { get; } = new();

        public void AddSkip(
            int rowNumber, string name, string email, string message,
            List<string>? warnings = null, bool hasFieldTypeError = false,
            bool hasLimitError = false, bool isDuplicate = false,
            bool isInactive = false, bool isArchived = false, bool hasInactiveField = false)
        {
            SkippedCount++;
            FailedRows.Add(rowNumber);
            Errors.Add($"Row {rowNumber}: {message}");
            RowPreviews.Add(new ImportRowPreview(
                rowNumber, name, email, "Skip", message, [], warnings ?? [],
                hasFieldTypeError, hasLimitError, isDuplicate, isInactive, isArchived, hasInactiveField));
        }

        public void AddUpdate(
            int rowNumber, string name, string email, List<FieldChange> changes,
            List<string> warnings, bool hasFieldTypeError, bool hasLimitError)
        {
            UpdatedCount++;
            RowPreviews.Add(new ImportRowPreview(
                rowNumber, name, email, "Update", null, changes, warnings,
                hasFieldTypeError, hasLimitError));
        }

        public void AddNew(
            int rowNumber, string name, string email, List<FieldChange> changes,
            List<string> warnings, bool hasFieldTypeError, bool hasLimitError)
        {
            ImportedCount++;
            RowPreviews.Add(new ImportRowPreview(
                rowNumber, name, email, "New", null, changes, warnings,
                hasFieldTypeError, hasLimitError));
        }
    }
}