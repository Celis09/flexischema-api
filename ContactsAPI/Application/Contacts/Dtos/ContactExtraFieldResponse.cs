namespace ContactsAPI.Application.Contacts.Dtos
{
    public class ContactExtraFieldResponse
    {
        public int ExtraFieldId { get; set; }
        public int ExtraFieldDefinitionId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;
        public string FieldValue { get; set; } = string.Empty;
    }
}
