using ContactsAPI.Application.Contacts.Dtos;
using ContactsAPI.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Application.Contacts.Queries.GetContactsPaged
{
    public class GetContactsHandler(ContactsDbContext context) : IRequestHandler<GetContactsQuery, List<ContactDto>>
    {
        private const int MaxRows = 500;

        public async Task<List<ContactDto>> Handle(GetContactsQuery request, CancellationToken cancellationToken)
        {
            var contacts = await context.Contacts
                .AsNoTracking()
                .Include(c => c.ExtraFields)
                    .ThenInclude(f => f.Definition)
                .Take(MaxRows)
                .ToListAsync(cancellationToken);

            return contacts.Select((c, index) => new ContactDto
            {
                Id = c.Id,
                Sequence = index + 1,
                Name = c.Name,
                Email = c.Email,
                ExtraFields = c.ExtraFields.Select(f => new ContactExtraFieldResponse
                {
                    ExtraFieldDefinitionId = f.ExtraFieldDefinitionId,
                    FieldValue = f.FieldValue
                }).ToList()
            }).ToList();
        }
    }
}