namespace ContactsAPI.Application.Admins.Commands.ImportContacts
{
    public record FieldChange(
        string FieldName,
        string? OldValue,
        string? NewValue
    );

    public record ImportRowPreview(
        int RowNumber,
        string Name,
        string Email,
        string Status,
        string? Reason,
        List<FieldChange> Changes,
        List<string> Warnings,
        bool IsInactive = false,
        bool IsArchived = false,
        bool HasInactiveField = false,
        bool HasFieldTypeError = false,
        bool IsDuplicateInBatch = false,
        bool HasLimitError = false
    );

    public record ImportResult(
        int ImportedCount,
        int UpdatedCount,
        int SkippedCount,
        List<int> FailedRows,
        List<string> Errors,
        List<ImportRowPreview> RowPreviews,
        bool DefinitionLimitReached = false,
        int DefinitionLimit = 0,
        int DefinitionSlotsRemaining = 0
    );
}