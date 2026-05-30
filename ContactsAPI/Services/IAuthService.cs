using ContactsAPI.Application.Auth;
using ContactsAPI.Application.Users.Dtos;
using ContactsAPI.Data;

namespace ContactsAPI.Services
{
    public interface IAuthService
    {
        UserDto? AuthenticateUser(LoginRequest request);
    }

    public class AuthService : IAuthService
    {
        private readonly ContactsDbContext _context;

        public AuthService(ContactsDbContext context)
        {
            _context = context;
        }

        public UserDto? AuthenticateUser(LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);
            if (user == null) return null;

            bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isValid) return null;

            return new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                Status = user.Status.ToString(),
                CreatedDate = user.CreatedDate
            };
        }

    }
}

