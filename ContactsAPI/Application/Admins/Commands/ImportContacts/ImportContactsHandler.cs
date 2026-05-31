using ContactsAPI.Application.Contacts.Commands.CreateContact;
using ContactsAPI.Application.Contacts.Dtos;
using ContactsAPI.Application.Helper;
using ContactsAPI.Data;
using ContactsAPI.Entities;
using ContactsAPI.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Application.Admins.Commands.ImportContacts
{
    public class ImportContactsHandler(
        ContactsDbContext context,
        IValidator<CreateContactCommand> contactValidator) : IRequestHandler<ImportContactsCommand, ImportResult>
    {
        public async Task<ImportResult> Handle(ImportContactsCommand request, CancellationToken ct)
        {
            var lines = request.CsvContent.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2)
                return new ImportResult(0, 0, 0, [], [], [], DefinitionLimitReached: false);

            var headers = CsvParser.ParseLine(lines[0]);
            var extraFieldNames = headers.Skip(2).ToHashSet(StringComparer.OrdinalIgnoreCase);

            // 1. Configuration & Definitions
            int maxLimit = await GetMaxDefinitionLimitAsync(ct);
            var lookups = await LoadDefinitionsAsync(extraFieldNames, ct);

            // 2. Auto-Create Missing Definitions
            var (limitReached, defsCreated, rejectedNames) = await AutoCreateMissingDefinitionsAsync(
                request, extraFieldNames, lookups, maxLimit, ct);

            lookups.RejectedDueToLimit = rejectedNames;

            // 3. Pre-load Contacts
            await PreloadExistingContactsAsync(lines, lookups, ct);

            // 4. Process Rows
            var tracker = new ImportTracker();
            for (int i = 1; i < lines.Length; i++)
            {
                await ProcessSingleRowAsync(i + 1, lines[i], headers, request, lookups, tracker, ct);
            }

            // 5. Persist Changes
            if (!request.DryRun)
            {
                if (tracker.NewContacts.Count > 0) context.Contacts.AddRange(tracker.NewContacts);
                if (tracker.NewExtraFields.Count > 0) context.ContactExtraFields.AddRange(tracker.NewExtraFields);
                await context.SaveChangesAsync(ct);
            }

            return new ImportResult(
                tracker.ImportedCount, tracker.UpdatedCount, tracker.SkippedCount,
                tracker.FailedRows, tracker.Errors, tracker.RowPreviews,
                DefinitionLimitReached: limitReached,
                DefinitionLimit: maxLimit,
                DefinitionSlotsRemaining: Math.Max(0, maxLimit - lookups.ActiveDefinitionCount - defsCreated)
            );
        }

        // ─── HELPER METHODS ────────────────────────────────────────────────────────

        private async Task<int> GetMaxDefinitionLimitAsync(CancellationToken ct)
        {
            var configEntry = await context.AdminConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Key == AdminConfigConstants.LimitConfigKey, ct);

            return configEntry != null && int.TryParse(configEntry.Value, out var parsed)
                ? parsed
                : AdminConfigConstants.AbsoluteMaxLimit;
        }

        private async Task<ImportLookups> LoadDefinitionsAsync(HashSet<string> extraFieldNames, CancellationToken ct)
        {
            var allDefinitions = await context.ExtraFieldDefinitions
                .Include(d => d.Options)
                .Where(d => extraFieldNames.Contains(d.FieldName) || d.IsRequired)
                .ToListAsync(ct);

            var lookups = new ImportLookups
            {
                DefinitionsByName = allDefinitions.Where(d => d.IsActive).ToDictionary(d => d.FieldName, d => d, StringComparer.OrdinalIgnoreCase),
                InactiveDefinitionNames = allDefinitions.Where(d => !d.IsActive).Select(d => d.FieldName).ToHashSet(StringComparer.OrdinalIgnoreCase),
                AllRequiredDefinitions = allDefinitions.Where(d => d.IsActive && d.IsRequired).ToList(),
                ActiveDefinitionCount = await context.ExtraFieldDefinitions.CountAsync(d => d.IsActive, ct)
            };

            lookups.DefinitionsById = lookups.DefinitionsByName.Values.ToDictionary(d => d.ExtraFieldDefinitionId, d => d);
            return lookups;
        }

        private async Task<(bool LimitReached, int CreatedCount, HashSet<string> Rejected)> AutoCreateMissingDefinitionsAsync(
            ImportContactsCommand request, HashSet<string> extraFieldNames, ImportLookups lookups, int maxLimit, CancellationToken ct)
        {
            var rejected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!request.AutoCreateDefinitions) return (false, 0, rejected);

            int availableSlots = Math.Max(0, maxLimit - lookups.ActiveDefinitionCount);
            var missingNames = extraFieldNames
                .Where(n => !lookups.DefinitionsByName.ContainsKey(n) && !lookups.InactiveDefinitionNames.Contains(n))
                .ToList();

            if (missingNames.Count == 0) return (false, 0, rejected);

            var namesToCreate = missingNames.Take(availableSlots).ToList();
            var namesToReject = missingNames.Skip(availableSlots).ToList();

            if (namesToReject.Count > 0)
            {
                foreach (var n in namesToReject) rejected.Add(n);
            }

            if (namesToCreate.Count > 0)
            {
                var newDefs = namesToCreate.Select((name, i) => new ExtraFieldDefinition
                {
                    ExtraFieldDefinitionId = request.DryRun ? -(lookups.DefinitionsByName.Count + i + 1) : 0,
                    FieldName = name,
                    FieldType = ExtraFieldType.Text,
                    IsActive = true
                }).ToList();

                if (!request.DryRun)
                {
                    context.ExtraFieldDefinitions.AddRange(newDefs);
                    await context.SaveChangesAsync(ct);
                }

                foreach (var def in newDefs)
                {
                    lookups.DefinitionsByName[def.FieldName] = def;
                    lookups.DefinitionsById[def.ExtraFieldDefinitionId] = def;
                }
            }

            return (namesToReject.Count > 0, namesToCreate.Count, rejected);
        }

        private async Task PreloadExistingContactsAsync(string[] lines, ImportLookups lookups, CancellationToken ct)
        {
            var csvEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 1; i < lines.Length; i++)
            {
                var p = CsvParser.ParseLine(lines[i]);
                if (p.Length >= 2)
                {
                    var emailRaw = p[1].Trim();
                    if (!csvEmails.Add(emailRaw)) lookups.DuplicateEmailsInCsv.Add(emailRaw);
                }
            }

            var existingContacts = await context.Contacts
                .Include(c => c.ExtraFields)
                .Where(c => c.Email != null && csvEmails.Contains(c.Email))
                .ToListAsync(ct);

            lookups.ExistingLookup = existingContacts.Where(c => c.Email != null).GroupBy(c => c.Email!).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
            lookups.InactiveContactEmails = existingContacts.Where(c => c.Status == ContactStatus.Inactive && c.Email != null).Select(c => c.Email!).ToHashSet(StringComparer.OrdinalIgnoreCase);
            lookups.ArchivedContactEmails = existingContacts.Where(c => c.Status == ContactStatus.Archived && c.Email != null).Select(c => c.Email!).ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private async Task ProcessSingleRowAsync(
            int rowNumber, string line, string[] headers, ImportContactsCommand request,
            ImportLookups lookups, ImportTracker tracker, CancellationToken ct)
        {
            var parts = CsvParser.ParseLine(line);
            if (parts.Length < 2)
            {
                tracker.AddSkip(rowNumber, "", "", "Not enough columns.");
                return;
            }

            var name = parts[0].Trim();
            var email = parts[1].Trim();

            // Row-level validations
            if (lookups.DuplicateEmailsInCsv.Contains(email))
            {
                tracker.AddSkip(rowNumber, name, email, $"Duplicate email in CSV: '{email}'. Only the first occurrence will be processed.", isDuplicate: true);
                lookups.DuplicateEmailsInCsv.Remove(email);
                return;
            }
            if (lookups.InactiveContactEmails.Contains(email))
            {
                tracker.AddSkip(rowNumber, name, email, "Contact exists but is inactive. Reactivate it before importing.", isInactive: true);
                return;
            }
            if (lookups.ArchivedContactEmails.Contains(email))
            {
                tracker.AddSkip(rowNumber, name, email, "Contact is archived and cannot be imported. Unarchive it first.", isArchived: true);
                return;
            }

            // Parse Extra Fields
            var (extraFields, rowError, warnings, hasFieldTypeError, hasLimitError, hasInactiveField) = ParseExtraFields(parts, headers, lookups);

            if (rowError != null)
            {
                tracker.AddSkip(rowNumber, name, email, rowError, hasInactiveField: hasInactiveField);
                return;
            }

            // Check Required Fields
            var missingRequired = lookups.AllRequiredDefinitions
                .Where(d => !extraFields.Any(ef => ef.ExtraFieldDefinitionId == d.ExtraFieldDefinitionId && !string.IsNullOrWhiteSpace(ef.FieldValue)))
                .Select(d => $"{d.FieldName} ({d.FieldType.ToLabel()})")
                .ToList();

            if (missingRequired.Count > 0)
            {
                tracker.AddSkip(rowNumber, name, email, $"Missing required field(s): {string.Join(", ", missingRequired)}.");
                return;
            }

            // Determine if Contact exists
            lookups.ProcessedInBatch.TryGetValue(email, out var existingContact);
            existingContact ??= lookups.ExistingLookup.GetValueOrDefault(email);
            bool isFromBatch = lookups.ProcessedInBatch.ContainsKey(email);

            if (existingContact != null)
            {
                UpdateExistingContact(rowNumber, name, email, existingContact, extraFields, request, lookups, tracker, warnings, hasFieldTypeError, hasLimitError, isFromBatch);
            }
            else
            {
                await CreateNewContactAsync(rowNumber, name, email, extraFields, request, lookups, tracker, warnings, hasFieldTypeError, hasLimitError, ct);
            }
        }

        private (List<ContactExtraFieldRequest> Fields, string? Error, List<string> Warnings, bool FieldTypeError, bool LimitError, bool InactiveField)
            ParseExtraFields(string[] parts, string[] headers, ImportLookups lookups)
        {
            var fields = new List<ContactExtraFieldRequest>();
            var warnings = new List<string>();
            bool fieldTypeError = false, limitError = false, inactiveField = false;

            for (int col = 2; col < headers.Length; col++)
            {
                var headerName = headers[col];
                var rawValue = col < parts.Length ? parts[col].Trim() : string.Empty;

                if (lookups.RejectedDueToLimit.Contains(headerName))
                {
                    warnings.Add($"Field '{headerName}' skipped — definition limit reached. Remove an existing definition to free up a slot.");
                    limitError = true;
                    continue;
                }

                if (lookups.InactiveDefinitionNames.Contains(headerName))
                    return (fields, $"Extra field '{headerName}' exists but is inactive. Reactivate it before importing.", warnings, fieldTypeError, limitError, true);

                if (!lookups.DefinitionsByName.TryGetValue(headerName, out var def))
                    return (fields, $"Extra field '{headerName}' is not defined. Enable 'Auto-create' to add it automatically.", warnings, fieldTypeError, limitError, false);

                if (string.IsNullOrWhiteSpace(rawValue))
                {
                    fields.Add(new ContactExtraFieldRequest { ExtraFieldDefinitionId = def.ExtraFieldDefinitionId, FieldValue = string.Empty });
                    continue;
                }

                var (normalizedValue, typeWarning) = ValidateAndNormalize(def, rawValue);
                if (typeWarning != null)
                {
                    warnings.Add(typeWarning);
                    fieldTypeError = true;
                }

                fields.Add(new ContactExtraFieldRequest { ExtraFieldDefinitionId = def.ExtraFieldDefinitionId, FieldValue = typeWarning != null ? rawValue : normalizedValue });
            }

            return (fields, null, warnings, fieldTypeError, limitError, inactiveField);
        }

        private void UpdateExistingContact(
            int rowNumber, string name, string email, Contact existingContact, List<ContactExtraFieldRequest> extraFields,
            ImportContactsCommand request, ImportLookups lookups, ImportTracker tracker, List<string> warnings,
            bool hasFieldTypeError, bool hasLimitError, bool isFromBatch)
        {
            var existingFieldMap = existingContact.ExtraFields.ToDictionary(ef => ef.ExtraFieldDefinitionId);
            var fieldChanges = new List<FieldChange>();

            foreach (var ef in extraFields)
            {
                if (!lookups.DefinitionsById.TryGetValue(ef.ExtraFieldDefinitionId, out var defn)) continue;

                if (existingFieldMap.TryGetValue(ef.ExtraFieldDefinitionId, out var existingField))
                {
                    bool valueChanged = !string.Equals(ef.FieldValue?.Trim(), existingField.FieldValue?.Trim(), StringComparison.OrdinalIgnoreCase);
                    bool shouldWrite = !string.IsNullOrWhiteSpace(ef.FieldValue) && valueChanged && (string.IsNullOrWhiteSpace(existingField.FieldValue) || request.OverwriteExisting);

                    if (shouldWrite)
                    {
                        fieldChanges.Add(new FieldChange(defn.FieldName, existingField.FieldValue, ef.FieldValue));
                        if (!request.DryRun) existingField.FieldValue = ef.FieldValue ?? string.Empty;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(ef.FieldValue))
                {
                    fieldChanges.Add(new FieldChange(defn.FieldName, null, ef.FieldValue));
                    if (!request.DryRun)
                    {
                        existingContact.ExtraFields.Add(new ContactExtraField
                        {
                            ContactId = existingContact.Id,
                            ExtraFieldDefinitionId = ef.ExtraFieldDefinitionId,
                            FieldValue = ef.FieldValue
                        });
                    }
                }
            }

            if (!isFromBatch) lookups.ProcessedInBatch[email] = existingContact;

            if (fieldChanges.Count > 0) tracker.AddUpdate(rowNumber, name, email, fieldChanges, warnings, hasFieldTypeError, hasLimitError);
            else tracker.AddSkip(rowNumber, name, email, "Contact exists; no new values to apply.", warnings, hasFieldTypeError, hasLimitError);
        }

        private async Task CreateNewContactAsync(
            int rowNumber, string name, string email, List<ContactExtraFieldRequest> extraFields,
            ImportContactsCommand request, ImportLookups lookups, ImportTracker tracker, List<string> warnings,
            bool hasFieldTypeError, bool hasLimitError, CancellationToken ct)
        {
            var fieldsForValidation = extraFields
                .Where(ef => !string.IsNullOrWhiteSpace(ef.FieldValue) && (!request.DryRun || ef.ExtraFieldDefinitionId > 0))
                .ToList();

            var validationResult = await contactValidator.ValidateAsync(new CreateContactCommand { Name = name, Email = email, ExtraFields = fieldsForValidation }, ct);

            if (!validationResult.IsValid)
            {
                tracker.AddSkip(rowNumber, name, email, string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)), warnings);
                return;
            }

            var contact = new Contact { Name = name, Email = email, ExtraFields = [] };
            var newFieldChanges = new List<FieldChange>();

            foreach (var ef in extraFields)
            {
                if (string.IsNullOrWhiteSpace(ef.FieldValue)) continue;

                var fieldName = lookups.DefinitionsById.TryGetValue(ef.ExtraFieldDefinitionId, out var d) ? d.FieldName : "Unknown";
                newFieldChanges.Add(new FieldChange(fieldName, null, ef.FieldValue));

                var field = new ContactExtraField { Contact = contact, ExtraFieldDefinitionId = ef.ExtraFieldDefinitionId, FieldValue = ef.FieldValue };
                contact.ExtraFields.Add(field);
                if (!request.DryRun) tracker.NewExtraFields.Add(field);
            }

            if (!request.DryRun) tracker.NewContacts.Add(contact);
            lookups.ProcessedInBatch[email] = contact;

            tracker.AddNew(rowNumber, name, email, newFieldChanges, warnings, hasFieldTypeError, hasLimitError);
        }

        private static (string Value, string? Warning) ValidateAndNormalize(ExtraFieldDefinition definition, string value)
        {
            var trimmed = value.Trim();

            switch (definition.FieldType)
            {
                case ExtraFieldType.Number:
                    if (!decimal.TryParse(trimmed, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _))
                        return (trimmed, $"Field '{definition.FieldName}' expects a number, got '{trimmed}'.");
                    return (trimmed, null);

                case ExtraFieldType.Date:
                    string[] dateFormats = ["MM/dd/yyyy", "M/d/yyyy", "M/dd/yyyy", "MM/d/yyyy", "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "dd/MM/yyyy", "d/M/yyyy"];
                    
                    if (DateTime.TryParseExact(trimmed, dateFormats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedDate))
                    {
                        return (parsedDate.ToString("yyyy-MM-dd"), null);
                    }
                    else if (DateTime.TryParse(trimmed, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var autoDate))
                    {
                        return (autoDate.ToString("yyyy-MM-dd"), null);
                    }
                    
                    return (trimmed, $"Field '{definition.FieldName}' expects a date, got '{trimmed}'. Supported formats include YYYY-MM-DD and MM/DD/YYYY.");

                case ExtraFieldType.Email:
                    if (!trimmed.Contains('@') || !trimmed.Contains('.'))
                        return (trimmed, $"Field '{definition.FieldName}' expects a valid email, got '{trimmed}'.");
                    return (trimmed.ToLowerInvariant(), null);

                case ExtraFieldType.Url:
                    if (!Uri.TryCreate(trimmed, UriKind.Absolute, out _))
                        return (trimmed, $"Field '{definition.FieldName}' expects a valid URL, got '{trimmed}'.");
                    return (trimmed, null);

                case ExtraFieldType.Phone:
                    if (!trimmed.All(c => char.IsDigit(c) || "+(). -".Contains(c)))
                        return (trimmed, $"Field '{definition.FieldName}' expects a valid phone number, got '{trimmed}'.");
                    return (trimmed, null);

                case ExtraFieldType.Option:
                    var matched = definition.Options.FirstOrDefault(o => string.Equals(o.OptionValue.Trim(), trimmed, StringComparison.OrdinalIgnoreCase));
                    if (matched == null)
                    {
                        var validOptions = string.Join(", ", definition.Options.OrderBy(o => o.DisplayOrder).Select(o => $"'{o.OptionValue}'"));
                        return (trimmed, $"Field '{definition.FieldName}' expects one of: {validOptions}. Got '{trimmed}'.");
                    }
                    return (matched.OptionValue, null);

                default:
                    return (trimmed, null);
            }
        }
    }
}