using ContactsAPI.Application.Attributes;
using ContactsAPI.Application.Users.Dtos;
using MediatR;

namespace ContactsAPI.Application.Users.Queries.GetUserById
{
    [AuthorizeRole("Admin")] // Only Admins can fetch user details
    public class GetUserByIdQuery : IRequest<UserDto?>
    {
        public int UserId { get; set; }
    }
}

