using ContactsAPI.Application.Attributes;
using ContactsAPI.Models;
using MediatR;

namespace ContactsAPI.Application.Users.Commands.ChangeUserStatus
{
    [AuthorizeRole("Admin")]
    public class ChangeUserStatusCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public UserStatus Status { get; set; }
    }
}
