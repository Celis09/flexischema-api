using ContactsAPI.Application.Attributes;
using ContactsAPI.Application.Contacts.Dtos;
using MediatR;

namespace ContactsAPI.Application.Contacts.Commands.CreateContact
{
    [AuthorizeRole("Admin", "Editor")]
    public class CreateContactCommand : IRequest<int>
    {
        public string Name { get; set; } = "";
        public string? Email { get; set; }
        public List<ContactExtraFieldRequest> ExtraFields { get; set; } = [];
    }
}