using ContactsAPI.Application.Abstractions;
using ContactsAPI.Application.Exceptions;
using ContactsAPI.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Application.Users.Commands.UpdateUser
{
    public class UpdateUserHandler(ContactsDbContext context, IUserContext userContext)
        : IRequestHandler<UpdateUserCommand, bool>
    {
        private static readonly HashSet<int> ProtectedUserIds = [1, 2];

        public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            // Guard 1 — Seeded demo accounts cannot be edited.
            if (ProtectedUserIds.Contains(request.UserId))
                throw new UnauthorizedAccessAppException(
                    "Seeded demo accounts cannot be edited.");

            // Guard 2 — A user cannot demote their own role.
            if (int.TryParse(userContext.UserId, out var currentUserId)
                && currentUserId == request.UserId
                && request.Role != "Admin")
            {
                throw new UnauthorizedAccessAppException(
                    "You cannot change your own role.");
            }

            var user = await context.Users
                .FindAsync([request.UserId], cancellationToken)
                ?? throw new NotFoundException($"User {request.UserId} not found");

            var isDuplicateUsername = await context.Users
                .AnyAsync(u => u.Username == request.Username && u.UserId != request.UserId, cancellationToken);

            if (isDuplicateUsername)
                throw new ValidationException(
                    new Dictionary<string, string[]>
                    {
                        { "Username", ["Username is already taken"] }
                    });

            var isDuplicateEmail = await context.Users
                .AnyAsync(u => u.Email == request.Email && u.UserId != request.UserId, cancellationToken);

            if (isDuplicateEmail)
                throw new ValidationException(
                    new Dictionary<string, string[]>
                    {
                        { "Email", ["Email is already registered"] }
                    });

            user.Username = request.Username;
            user.Email = request.Email;
            user.Role = request.Role;

            if (!string.IsNullOrEmpty(request.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}