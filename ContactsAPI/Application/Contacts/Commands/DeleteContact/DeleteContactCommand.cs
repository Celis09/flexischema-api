using ContactsAPI.Application.Attributes;
using MediatR;

namespace ContactsAPI.Application.Contacts.Commands.DeleteContact
{
    [AuthorizeRole("Admin")]
    public class DeleteContactCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}
