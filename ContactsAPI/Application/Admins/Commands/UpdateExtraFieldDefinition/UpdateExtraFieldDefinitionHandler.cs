using ContactsAPI.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Application.Admins.Commands.UpdateExtraFieldDefinition
{
    public class UpdateExtraFieldDefinitionHandler(ContactsDbContext context) : IRequestHandler<UpdateExtraFieldDefinitionCommand, bool>
    {
        public async Task<bool> Handle(UpdateExtraFieldDefinitionCommand request, CancellationToken cancellationToken)
        {
            var definition = await context.ExtraFieldDefinitions
                .FirstOrDefaultAsync(d => d.ExtraFieldDefinitionId == request.ExtraFieldDefinitionId, cancellationToken);

            if (definition == null) return false;

            definition.FieldName = request.FieldName;
            definition.FieldType = request.FieldType;
            definition.IsActive = request.IsActive;
            definition.IsRequired = request.IsRequired;

            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}