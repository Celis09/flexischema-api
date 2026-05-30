using ContactsAPI.Data;
using ContactsAPI.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Application.Contacts.Queries.ExportContacts
{
    public class ExportContactsHandler(
        ContactsDbContext context,
        IContactExportService exportService) : IRequestHandler<ExportContactsQuery, ExportResult>
    {
        public async Task<ExportResult> Handle(ExportContactsQuery request, CancellationToken ct)
        {
            // 1. Fetch Configuration
            var definitions = await context.ExtraFieldDefinitions
                .Where(d => d.IsActive)
                .OrderBy(d => d.FieldName)
                .ToListAsync(ct);

            // 2. Parse Requested IDs
            var requestedIds = ParseRequestedIds(request.Ids);

            // 3. Fetch Data
            var query = context.Contacts
                .Include(c => c.ExtraFields)
                    .ThenInclude(ef => ef.Definition)
                .AsQueryable();

            if (requestedIds?.Count > 0)
            {
                query = query.Where(c => requestedIds.Contains(c.Id));
            }

            var contacts = await query.ToListAsync(ct);

            if (contacts.Count == 0)
            {
                return new ExportResult("No contacts found.", "text/plain", "empty.txt");
            }

            // 4. Delegate to the Export Service
            return exportService.GenerateExport(request, contacts, definitions, requestedIds?.Count > 0);
        }

        private static HashSet<int>? ParseRequestedIds(string? idsString)
        {
            if (string.IsNullOrWhiteSpace(idsString)) return null;

            return idsString
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => int.TryParse(s, out var n) ? (int?)n : null)
                .Where(n => n.HasValue)
                .Select(n => n!.Value)
                .ToHashSet();
        }
    }
}