using ContactsAPI.Entities;

namespace ContactsAPI.Application.Admins.Commands.ImportContacts
{
    public class ImportLookups
    {
        public Dictionary<string, ExtraFieldDefinition> DefinitionsByName { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<int, ExtraFieldDefinition> DefinitionsById { get; set; } = new();
        public HashSet<string> InactiveDefinitionNames { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public List<ExtraFieldDefinition> AllRequiredDefinitions { get; set; } = new();
        public int ActiveDefinitionCount { get; set; }

        public HashSet<string> RejectedDueToLimit { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> DuplicateEmailsInCsv { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> InactiveContactEmails { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> ArchivedContactEmails { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, Contact> ExistingLookup { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, Contact> ProcessedInBatch { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}