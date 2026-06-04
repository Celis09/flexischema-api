using ContactsAPI.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

            // Invalidate cached AI insight — status change may affect AI analysis
            var cachedInsight = await context.ContactInsights
                .FirstOrDefaultAsync(ci => ci.ContactId == contact.Id, cancellationToken);
            if (cachedInsight != null)
            {
                context.ContactInsights.Remove(cachedInsight);
                await context.SaveChangesAsync(cancellationToken);
            }

            return true;
        }
    }
}