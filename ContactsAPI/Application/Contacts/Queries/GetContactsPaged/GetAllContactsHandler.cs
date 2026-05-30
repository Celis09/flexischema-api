using ContactsAPI.Application.Contacts.Dtos;
using ContactsAPI.Application.Extensions;
using ContactsAPI.Application.Helper;
using ContactsAPI.Data;
using ContactsAPI.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Application.Contacts.Queries.GetContactsPaged
{
    public class GetAllContactsHandler(ContactsDbContext context)
        : IRequestHandler<GetAllContactsQuery, PagedResult<ContactDto>>
    {
        public async Task<PagedResult<ContactDto>> Handle(GetAllContactsQuery request, CancellationToken cancellationToken)
        {
            var query = context.Contacts
                .AsNoTracking()
                .Include(c => c.ExtraFields)
                    .ThenInclude(f => f.Definition)
                .AsQueryable()
                .ApplySearch(request.Search);

            if (request.Status.HasValue)
                query = query.Where(c => c.Status == request.Status.Value);

            if (!request.IsAdmin)
                query = query.Where(c => c.Status == ContactStatus.Active);

            if (request.FromDate.HasValue)
                query = query.Where(c => c.CreatedDate >= request.FromDate.Value.Date);

            if (request.ToDate.HasValue)
                query = query.Where(c => c.CreatedDate < request.ToDate.Value.Date.AddDays(1));

            // ApplySorting now safely adds the OrderBy AND the ThenBy(c => c.Id)
            query = query.ApplySorting(request.SortBy, request.SortOrder);

            var totalCount = await query.CountAsync(cancellationToken);

            var contacts = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var items = contacts.Select((c, index) => new ContactDto
            {
                Id = c.Id,
                Sequence = ((request.Page - 1) * request.PageSize) + index + 1,
                Name = c.Name,
                Email = c.Email,
                CreatedDate = c.CreatedDate,
                Status = c.Status.ToString(),
                ExtraFields = c.ExtraFields
                    .Where(f => f.Definition.IsActive)
                    .Select(f => new ContactExtraFieldResponse
                    {
                        ExtraFieldId = f.ExtraFieldId,
                        ExtraFieldDefinitionId = f.ExtraFieldDefinitionId,
                        FieldName = f.Definition.FieldName,
                        FieldType = f.Definition.FieldType.ToString(),
                        FieldValue = f.FieldValue
                    }).ToList()
            }).ToList();

            return new PagedResult<ContactDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}