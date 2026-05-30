namespace ContactsAPI.Entities
{
    public class ContactExtraField
    {
        public int ExtraFieldId { get; set; }
        public int ContactId { get; set; }
        public int ExtraFieldDefinitionId { get; set; }
        public string FieldValue { get; set; } = string.Empty;
        public Contact Contact { get; set; } = null!;
        public ExtraFieldDefinition Definition { get; set; } = null!;

    }
}
