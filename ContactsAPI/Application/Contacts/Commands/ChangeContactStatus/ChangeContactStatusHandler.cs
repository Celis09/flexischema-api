using ContactsAPI.Data;
using MediatR;

namespace ContactsAPI.Application.Contacts.Commands.ChangeContactStatus
{
    public class ChangeContactStatusHandler(ContactsDbContext context) : IRequestHandler<ChangeContactStatusCommand, bool>
    {
        public async Task<bool> Handle(ChangeContactStatusCommand request, CancellationToken cancellationToken)
        {
            var contact = await context.Contacts
                .FindAsync([request.Id], cancellationToken);

            if (contact == null) return false;

            contact.Status = request.Status;
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}