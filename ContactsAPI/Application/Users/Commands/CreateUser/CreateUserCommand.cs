using ContactsAPI.Application.Attributes;
using MediatR;

namespace ContactsAPI.Application.Users.Commands.CreateUser
{
    [AuthorizeRole("Admin")]
    public class CreateUserCommand : IRequest<int>
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Editor";
        public string Password { get; set; } = string.Empty;
    }
}
