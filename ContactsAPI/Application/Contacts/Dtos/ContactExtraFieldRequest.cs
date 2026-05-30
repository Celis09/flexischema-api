namespace ContactsAPI.Application.Contacts.Dtos
{
    public class ContactExtraFieldRequest
    {
        //DTO for adding and updating contact
        public int ExtraFieldDefinitionId { get; set; }
        public string FieldValue { get; set; } = string.Empty;
    }
}
