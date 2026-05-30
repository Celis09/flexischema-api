using ContactsAPI.Data;
using ContactsAPI.Entities;
using MediatR;

namespace ContactsAPI.Application.Admins.Commands.AddExtraFieldDefinition
{
    public class AddExtraFieldDefinitionHandler(ContactsDbContext context) : IRequestHandler<AddExtraFieldDefinitionCommand, int>
    {
        public async Task<int> Handle(AddExtraFieldDefinitionCommand request, CancellationToken cancellationToken)
        {
            var definition = new ExtraFieldDefinition
            {
                FieldName = request.FieldName,
                FieldType = request.FieldType,
                IsActive = request.IsActive,
                IsRequired = request.IsRequired,
            };

            context.ExtraFieldDefinitions.Add(definition);
            await context.SaveChangesAsync(cancellationToken);
            return definition.ExtraFieldDefinitionId;
        }
    }
}