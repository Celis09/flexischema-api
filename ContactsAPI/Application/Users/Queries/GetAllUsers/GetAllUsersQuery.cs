using ContactsAPI.Application.Attributes;
using ContactsAPI.Application.Contacts.Dtos;
using ContactsAPI.Application.Users.Dtos;
using ContactsAPI.Models;
using MediatR;

namespace ContactsAPI.Application.Users.Queries.GetAllUsers
{
    [AuthorizeRole("Admin")]
    public class GetAllUsersQuery : IRequest<PagedResult<UserDto>>
    {
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public UserStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}

