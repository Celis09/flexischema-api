using ContactsAPI.Data;
using ContactsAPI.Entities;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace ContactsAPI.Application.Admins.Commands.AddExtraFieldDefinition
{
    public class AddExtraFieldDefinitionHandler(ContactsDbContext context, IMemoryCache cache) : IRequestHandler<AddExtraFieldDefinitionCommand, int>
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
            cache.Remove("extrafield:definitions:withOptions");
            return definition.ExtraFieldDefinitionId;
        }
    }
}