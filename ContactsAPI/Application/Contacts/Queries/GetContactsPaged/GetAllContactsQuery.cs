using ContactsAPI.Application.Contacts.Dtos;
using ContactsAPI.Entities;
using MediatR;

namespace ContactsAPI.Application.Contacts.Queries.GetContactsPaged
{
    public record GetAllContactsQuery(
    string? Search,
    int Page = 1,
    int PageSize = 20,
    string SortBy = "Name",
    string SortOrder = "asc",
    DateTime? FromDate = null,   // optional filter
    DateTime? ToDate = null,      // optional filter
    ContactStatus? Status = null, // new optional filter
    bool IsAdmin = false,
    bool IsEditor = false
) : IRequest<PagedResult<ContactDto>>;
}
