using ContactsAPI.Application.Contacts.Queries.ExportContacts;
using ContactsAPI.Entities;
using System.Text;
using System.Text.Json;

namespace ContactsAPI.Services
{
    public class ContactExportService : IContactExportService
    {
        private const long MaxExportBytes = 5 * 1024 * 1024;
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public ExportResult GenerateExport(
            ExportContactsQuery request,
            List<Contact> contacts,
            List<ExtraFieldDefinition> definitions,
            bool isSelective)
        {
            var effectiveCols = DetermineEffectiveColumns(request, definitions);

            return request.Format.ToLower() switch
            {
                "json" => GenerateJsonExport(contacts, definitions, effectiveCols),
                "csv" or _ => GenerateCsvExport(contacts, definitions, effectiveCols, isSelective)
            };
        }

        private static HashSet<string> DetermineEffectiveColumns(ExportContactsQuery request, List<ExtraFieldDefinition> definitions)
        {
            HashSet<string> allowedBuiltins = request.IsAdmin
                ? ["id", "name", "email", "status", "createdDate"]
                : request.IsEditor
                    ? ["id", "name", "email"]
                    : ["name", "email"];

            HashSet<string> allAllowed = [.. allowedBuiltins, .. definitions.Select(d => $"extra-{d.ExtraFieldDefinitionId}")];

            if (string.IsNullOrWhiteSpace(request.Columns))
            {
                return allAllowed;
            }

            var requested = request.Columns
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToHashSet();

            return [.. requested.Intersect(allAllowed)];
        }

        private static ExportResult GenerateJsonExport(List<Contact> contacts, List<ExtraFieldDefinition> definitions, HashSet<string> cols)
        {
            var json = JsonSerializer.Serialize(
                contacts.Select(c => BuildJsonRow(c, definitions, cols)),
                JsonOptions
            );

            if (Encoding.UTF8.GetByteCount(json) > MaxExportBytes)
            {
                return new ExportResult("Export too large. Apply filters to reduce the result set.", "text/plain", "too-large.txt");
            }

            return new ExportResult(json, "application/json", "contacts.json");
        }

        private static ExportResult GenerateCsvExport(List<Contact> contacts, List<ExtraFieldDefinition> definitions, HashSet<string> cols, bool isSelective)
        {
            var csv = new StringBuilder();
            long byteCount = 0;

            var extraDefsToInclude = definitions
                .Where(d => cols.Contains($"extra-{d.ExtraFieldDefinitionId}"))
                .ToList();

            bool Include(string key) => cols.Contains(key);

            // Build Headers
            List<string> headers =
            [
                ..Include("id") ? (string[])["ID"] : [],
                ..Include("name") ? (string[])["Name"] : [],
                ..Include("email") ? (string[])["Email"] : [],
                ..Include("status") ? (string[])["Status"] : [],
                ..Include("createdDate") ? (string[])["Created Date"] : [],
                ..extraDefsToInclude.Select(d => CsvEscape(d.FieldName))
            ];

            var headerLine = string.Join(",", headers) + "\r\n";
            csv.Append(headerLine);
            byteCount += Encoding.UTF8.GetByteCount(headerLine);

            // Build Rows
            foreach (var contact in contacts)
            {
                List<string> row =
                [
                    ..Include("id") ? (string[])[CsvEscape(contact.Id.ToString())] : [],
                    ..Include("name") ? (string[])[CsvEscape(contact.Name)] : [],
                    ..Include("email") ? (string[])[CsvEscape(contact.Email ?? "")] : [],
                    ..Include("status") ? (string[])[CsvEscape(contact.Status.ToString())] : [],
                    ..Include("createdDate") ? (string[])[CsvEscape(contact.CreatedDate.ToString("yyyy-MM-dd"))] : [],
                    ..extraDefsToInclude.Select(def =>
                        CsvEscape(contact.ExtraFields
                            .FirstOrDefault(f => f.ExtraFieldDefinitionId == def.ExtraFieldDefinitionId)
                            ?.FieldValue ?? ""))
                ];

                var line = string.Join(",", row) + "\r\n";
                byteCount += Encoding.UTF8.GetByteCount(line);

                if (byteCount > MaxExportBytes)
                {
                    return new ExportResult("Export too large. Apply filters or select fewer columns to reduce the result set.", "text/plain", "too-large.txt");
                }

                csv.Append(line);
            }

            var fileName = isSelective
                ? $"contacts-selected-{DateTime.UtcNow:yyyyMMdd}.csv"
                : $"contacts-{DateTime.UtcNow:yyyyMMdd}.csv";

            return new ExportResult(csv.ToString(), "text/csv", fileName);
        }

        private static string CsvEscape(string value)
        {
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }

        private static Dictionary<string, object?> BuildJsonRow(Contact contact, List<ExtraFieldDefinition> definitions, HashSet<string> cols)
        {
            var dict = new Dictionary<string, object?>();
            if (cols.Contains("id")) dict["Id"] = contact.Id;
            if (cols.Contains("name")) dict["Name"] = contact.Name;
            if (cols.Contains("email")) dict["Email"] = contact.Email;
            if (cols.Contains("status")) dict["Status"] = contact.Status.ToString();
            if (cols.Contains("createdDate")) dict["CreatedDate"] = contact.CreatedDate.ToString("yyyy-MM-dd");

            var extraFields = definitions
                .Where(d => cols.Contains($"extra-{d.ExtraFieldDefinitionId}"))
                .Select(d => new
                {
                    d.FieldName,
                    FieldValue = contact.ExtraFields
                        .FirstOrDefault(ef => ef.ExtraFieldDefinitionId == d.ExtraFieldDefinitionId)
                        ?.FieldValue ?? ""
                })
                .ToList();

            if (extraFields.Count > 0)
                dict["ExtraFields"] = extraFields;

            return dict;
        }
    }
}