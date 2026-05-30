using ContactsAPI.Application.Attributes;
using MediatR;

namespace ContactsAPI.Application.Users.Commands.UpdateUser
{
    [AuthorizeRole("Admin")]
    public class UpdateUserCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Editor";
        public string Password { get; set; } = string.Empty; // plain text from request
    }
}

