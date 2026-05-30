using ContactsAPI.Entities;

namespace ContactsAPI.Models
{
    public class ExtraFieldOption
    {
        public int ExtraFieldOptionId { get; set; }
        public int ExtraFieldDefinitionId { get; set; }
        public string OptionValue { get; set; } = string.Empty;
        public int DisplayOrder { get; set; } = 0;
        public ExtraFieldDefinition Definition { get; set; } = null!;
    }
}