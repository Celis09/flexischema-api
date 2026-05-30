using ContactsAPI.Application.Abstractions;
using ContactsAPI.Application.Exceptions;
using ContactsAPI.Data;
using ContactsAPI.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Application.Users.Commands.ChangeUserStatus
{
    public class ChangeUserStatusHandler(ContactsDbContext context, IUserContext userContext)
        : IRequestHandler<ChangeUserStatusCommand, bool>
    {
        private static readonly HashSet<int> ProtectedUserIds = [1, 2];

        public async Task<bool> Handle(ChangeUserStatusCommand request, CancellationToken cancellationToken)
        {
            if (ProtectedUserIds.Contains(request.UserId))
                throw new UnauthorizedAccessAppException(
                    "The status of seeded demo accounts cannot be changed.");

            if (int.TryParse(userContext.UserId, out var currentUserId)
                && currentUserId == request.UserId
                && request.Status != UserStatus.Active)
            {
                throw new UnauthorizedAccessAppException(
                    "You cannot set your own status to Inactive or Suspended.");
            }

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);

            if (user == null)
                return false;

            user.Status = request.Status;

            context.Users.Update(user);
            await context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}