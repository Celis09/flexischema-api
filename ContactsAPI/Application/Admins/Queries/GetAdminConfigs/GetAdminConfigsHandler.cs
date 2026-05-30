using ContactsAPI.Application.Admins.Dtos;
using ContactsAPI.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Application.Admins.Queries.GetAdminConfigs
{
    public class GetAdminConfigsHandler(ContactsDbContext context) : IRequestHandler<GetAdminConfigsQuery, List<AdminConfigDto>>
    {
        public async Task<List<AdminConfigDto>> Handle(GetAdminConfigsQuery request, CancellationToken ct)
        {
            return await context.AdminConfigs
                .Select(c => new AdminConfigDto
                {
                    Id = c.Id,
                    Key = c.Key,
                    Value = c.Value,
                    Description = c.Description
                })
                .ToListAsync(ct);
        }
    }
}