using MediatR;

namespace ContactsAPI.Application.Admins.Commands.ChangeExtraFieldDefinitionRequiredStatus
{
    public class ChangeExtraFieldDefinitionRequiredStatusCommand : IRequest<bool>
    {
        public int ExtraFieldDefinitionId { get; set; }
        public bool IsRequired { get; set; }
    }
}
