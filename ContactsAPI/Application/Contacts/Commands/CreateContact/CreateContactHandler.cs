using ContactsAPI.Data;
using ContactsAPI.Entities;
using MediatR;

namespace ContactsAPI.Application.Contacts.Commands.CreateContact
{
    /// <summary>
    /// CQRS Command Handler for creating a new Contact.
    /// This encapsulates the specific business logic for creating a contact so controllers remain thin.
    /// </summary>
    public class CreateContactHandler(ContactsDbContext context) : IRequestHandler<CreateContactCommand, int>
    {
        public async Task<int> Handle(CreateContactCommand request, CancellationToken cancellationToken)
        {
            var contact = new Contact
            {
                Name = request.Name,
                Email = request.Email,
                Status = ContactStatus.Active,
                CreatedDate = DateTime.UtcNow,
                ExtraFields = [.. request.ExtraFields.Select(f => new ContactExtraField
                {
                    ExtraFieldDefinitionId = f.ExtraFieldDefinitionId,
                    FieldValue = f.FieldValue
                })]
            };

            context.Contacts.Add(contact);
            await context.SaveChangesAsync(cancellationToken);
            return contact.Id;
        }
    }
}