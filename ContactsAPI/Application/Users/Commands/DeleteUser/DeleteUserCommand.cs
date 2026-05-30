using ContactsAPI.Application.Attributes;
using MediatR;

namespace ContactsAPI.Application.Users.Commands.DeleteUser
{
    [AuthorizeRole("Admin")]
    public class DeleteUserCommand : IRequest<bool>
    {
        public int UserId { get; set; }
    }
}
