using MediatR;

namespace ContactsAPI.Application.Admins.Commands.ChangeExtraFieldDefinitionActiveStatus
{
    public class ChangeExtraFieldDefinitionActiveStatusCommand : IRequest<bool>
    {
        public int ExtraFieldDefinitionId { get; set; }
        public bool IsActive { get; set; }
    }
}
