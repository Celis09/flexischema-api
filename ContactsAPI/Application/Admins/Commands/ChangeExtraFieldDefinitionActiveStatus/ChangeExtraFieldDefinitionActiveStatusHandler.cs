using ContactsAPI.Data;
using MediatR;

namespace ContactsAPI.Application.Admins.Commands.ChangeExtraFieldDefinitionActiveStatus
{
    public class ChangeExtraFieldDefinitionActiveStatusHandler(ContactsDbContext context) : IRequestHandler<ChangeExtraFieldDefinitionActiveStatusCommand, bool>
    {
        public async Task<bool> Handle(ChangeExtraFieldDefinitionActiveStatusCommand request, CancellationToken cancellationToken)
        {
            var definition = await context.ExtraFieldDefinitions
                .FindAsync([request.ExtraFieldDefinitionId], cancellationToken);

            if (definition == null) return false;

            definition.IsActive = request.IsActive;
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}