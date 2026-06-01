using ContactsAPI.Application.Contacts.Dtos;
using ContactsAPI.Data;
using ContactsAPI.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Application.Contacts.Queries.GetContactById
{
    public class GetContactByIdHandler(ContactsDbContext context) : IRequestHandler<GetContactByIdQuery, ContactDto?>
    {
        public async Task<ContactDto?> Handle(GetContactByIdQuery request, CancellationToken cancellationToken)
        {
            var query = context.Contacts
                .Include(c => c.ExtraFields)
                    .ThenInclude(f => f.Definition)
                .AsQueryable();

            if (!request.IsAdmin)
            {
                query = query.Where(c => c.Status == ContactStatus.Active);
            }

            var contact = await query.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (contact == null) return null;

            return new ContactDto
            {
                Id = contact.Id,
                Sequence = 1,
                Name = contact.Name,
                Email = contact.Email,
                ExtraFields = contact.ExtraFields.Select(f => new ContactExtraFieldResponse
                {
                    ExtraFieldId = f.ExtraFieldId,
                    ExtraFieldDefinitionId = f.ExtraFieldDefinitionId,
                    FieldValue = f.FieldValue,
                    FieldName = f.Definition?.FieldName ?? string.Empty,
                    FieldType = f.Definition?.FieldType.ToString() ?? string.Empty
                }).ToList()
            };
        }
    }
}