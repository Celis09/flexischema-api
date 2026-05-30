using ContactsAPI.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Application.Contacts.Commands.DeleteContact
{
    public class DeleteContactHandler(ContactsDbContext context) : IRequestHandler<DeleteContactCommand, bool>
    {
        public async Task<bool> Handle(DeleteContactCommand request, CancellationToken cancellationToken)
        {
            var contact = await context.Contacts
                .Include(c => c.ExtraFields)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (contact == null) return false;

            context.ContactExtraFields.RemoveRange(contact.ExtraFields);
            context.Contacts.Remove(contact);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}