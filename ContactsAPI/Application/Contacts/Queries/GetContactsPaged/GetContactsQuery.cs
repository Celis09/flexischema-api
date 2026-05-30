using ContactsAPI.Application.Contacts.Dtos;
using MediatR;

namespace ContactsAPI.Application.Contacts.Queries.GetContactsPaged
{
    public class GetContactsQuery : IRequest<List<ContactDto>>
    {
    }
}
