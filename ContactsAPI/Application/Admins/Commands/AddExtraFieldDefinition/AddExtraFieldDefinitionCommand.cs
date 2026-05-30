using ContactsAPI.Application.Attributes;
using ContactsAPI.Models;
using MediatR;

namespace ContactsAPI.Application.Admins.Commands.AddExtraFieldDefinition
{
    [AuthorizeRole("Admin")]
    public class AddExtraFieldDefinitionCommand : IRequest<int>
    {
        public string FieldName { get; set; } = string.Empty;
        public ExtraFieldType FieldType { get; set; } = ExtraFieldType.Text;
        public bool IsRequired { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }
}