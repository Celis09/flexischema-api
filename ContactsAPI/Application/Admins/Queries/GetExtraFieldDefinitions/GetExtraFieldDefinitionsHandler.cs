using ContactsAPI.Application.Admins.Dtos;
using ContactsAPI.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Application.Admins.Queries.GetExtraFieldDefinitions
{
    public class GetExtraFieldDefinitionsHandler : IRequestHandler<GetExtraFieldDefinitionsQuery, List<ExtraFieldDefinitionDto>>
    {
        private readonly ContactsDbContext _context;

        public GetExtraFieldDefinitionsHandler(ContactsDbContext context)
        {
            _context = context;
        }

        public async Task<List<ExtraFieldDefinitionDto>> Handle(GetExtraFieldDefinitionsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ExtraFieldDefinitions.AsQueryable();

            if (request.IsActive.HasValue)
                query = query.Where(d => d.IsActive == request.IsActive.Value);

            return await query.Select(d => new ExtraFieldDefinitionDto
            {
                ExtraFieldDefinitionId = d.ExtraFieldDefinitionId,
                FieldName = d.FieldName,
                FieldType = d.FieldType.ToString(),
                IsRequired = d.IsRequired,
                IsActive = d.IsActive,
                Options = d.Options
                  .OrderBy(o => o.DisplayOrder)
                  .Select(o => o.OptionValue)
                  .ToList()
            }).ToListAsync(cancellationToken);
        }
    }
}
