using ContactsAPI.Data;
using MediatR;

namespace ContactsAPI.Application.Admins.Commands.ChangeExtraFieldDefinitionRequiredStatus
{
    public class ChangeExtraFieldDefinitionRequiredStatusHandler(ContactsDbContext context) : IRequestHandler<ChangeExtraFieldDefinitionRequiredStatusCommand, bool>
    {
        public async Task<bool> Handle(ChangeExtraFieldDefinitionRequiredStatusCommand request, CancellationToken cancellationToken)
        {
            var definition = await context.ExtraFieldDefinitions
                .FindAsync([request.ExtraFieldDefinitionId], cancellationToken);

            if (definition == null) return false;

            definition.IsRequired = request.IsRequired;
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}