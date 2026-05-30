namespace ContactsAPI.Application.Admins.Dtos
{
    public class ExtraFieldDefinitionDto
    {
        public int ExtraFieldDefinitionId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; }
        public List<string> Options { get; set; } = [];
    }
}