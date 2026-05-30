using ContactsAPI.Models;

namespace ContactsAPI.Entities
{
    public class ExtraFieldDefinition
    {
        public int ExtraFieldDefinitionId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public ExtraFieldType FieldType { get; set; } = ExtraFieldType.Text;
        public bool IsRequired { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public ICollection<ContactExtraField> ContactExtraFields { get; set; } = new List<ContactExtraField>();
        public ICollection<ExtraFieldOption> Options { get; set; } = new List<ExtraFieldOption>();
    }
}