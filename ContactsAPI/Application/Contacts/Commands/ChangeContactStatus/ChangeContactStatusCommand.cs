using ContactsAPI.Application.Attributes;
using ContactsAPI.Entities;
using MediatR;

namespace ContactsAPI.Application.Contacts.Commands.ChangeContactStatus
{
    [AuthorizeRole("Admin")]
    public class ChangeContactStatusCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public ContactStatus Status { get; set; }
    }

}
