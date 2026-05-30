using ContactsAPI.Application.Attributes;
using ContactsAPI.Models;
using MediatR;

namespace ContactsAPI.Application.Admins.Commands.UpdateExtraFieldDefinition
{
    [AuthorizeRole("Admin")]
    public class UpdateExtraFieldDefinitionCommand : IRequest<bool>
    {
        public int ExtraFieldDefinitionId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public ExtraFieldType FieldType { get; set; } = ExtraFieldType.Text;
        public bool IsRequired { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }
}