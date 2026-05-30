using ContactsAPI.Data;
using ContactsAPI.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Application.Contacts.Commands.UpdateContact
{
    public class UpdateContactHandler(ContactsDbContext context) : IRequestHandler<UpdateContactCommand, bool>
    {
        public async Task<bool> Handle(UpdateContactCommand request, CancellationToken cancellationToken)
        {
            var contact = await context.Contacts
                .Include(c => c.ExtraFields)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (contact == null) return false;

            contact.Name = request.Name;
            contact.Email = request.Email;

            foreach (var ef in request.ExtraFields)
            {
                var existing = contact.ExtraFields
                    .FirstOrDefault(x => x.ExtraFieldDefinitionId == ef.ExtraFieldDefinitionId);

                if (existing != null)
                {
                    existing.FieldValue = ef.FieldValue;
                }
                else
                {
                    contact.ExtraFields.Add(new ContactExtraField
                    {
                        ExtraFieldDefinitionId = ef.ExtraFieldDefinitionId,
                        FieldValue = ef.FieldValue,
                        ContactId = contact.Id
                    });
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}