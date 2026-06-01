using ContactsAPI.Application.Contacts.Dtos;
using MediatR;

namespace ContactsAPI.Application.Contacts.Queries.GetContactById
{
    public class GetContactByIdQuery : IRequest<ContactDto?>
    {
        public int Id { get; set; }
        public bool IsAdmin { get; set; }
    }
}
