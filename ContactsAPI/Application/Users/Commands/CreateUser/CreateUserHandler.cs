using ContactsAPI.Data;
using ContactsAPI.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Application.Users.Commands.CreateUser
{
    public class CreateUserHandler(ContactsDbContext context) : IRequestHandler<CreateUserCommand, int>
    {
        public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var isDuplicateUsername = await context.Users
                .AnyAsync(u => u.Username == request.Username, cancellationToken);

            if (isDuplicateUsername)
                throw new ContactsAPI.Application.Exceptions.ValidationException(
                    new Dictionary<string, string[]>
                    {
                    { "Username", ["Username is already taken"] }
                    });

            var isDuplicateEmail = await context.Users
                .AnyAsync(u => u.Email == request.Email, cancellationToken);

            if (isDuplicateEmail)
                throw new ContactsAPI.Application.Exceptions.ValidationException(
                    new Dictionary<string, string[]>
                    {
                    { "Email", ["Email is already registered"] }
                    });

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = hashedPassword,
                Role = request.Role,
                Status = UserStatus.Active,
                CreatedDate = DateTime.UtcNow
            };

            context.Users.Add(user);
            await context.SaveChangesAsync(cancellationToken);
            return user.UserId;
        }
    }
}