using ContactsAPI.Data;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace ContactsAPI.Application.Admins.Commands.ChangeExtraFieldDefinitionRequiredStatus
{
    public class ChangeExtraFieldDefinitionRequiredStatusHandler(ContactsDbContext context, IMemoryCache cache) : IRequestHandler<ChangeExtraFieldDefinitionRequiredStatusCommand, bool>
    {
        public async Task<bool> Handle(ChangeExtraFieldDefinitionRequiredStatusCommand request, CancellationToken cancellationToken)
        {
            var definition = await context.ExtraFieldDefinitions
                .FindAsync([request.ExtraFieldDefinitionId], cancellationToken);

            if (definition == null) return false;

            definition.IsRequired = request.IsRequired;
            await context.SaveChangesAsync(cancellationToken);
            cache.Remove("extrafield:definitions:withOptions");
            return true;
        }
    }
}