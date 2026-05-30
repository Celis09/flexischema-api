using ContactsAPI.Application.Attributes;
using ContactsAPI.Application.Contacts.Dtos;
using MediatR;

namespace ContactsAPI.Application.Contacts.Commands.UpdateContact
{
    [AuthorizeRole("Admin", "Editor")]
    public class UpdateContactCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Email { get; set; }

        public List<ContactExtraFieldRequest> ExtraFields { get; set; } = [];
    }

}
