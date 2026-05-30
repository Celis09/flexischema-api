using ContactsAPI.Application.Contacts.Dtos;
using ContactsAPI.Application.Extensions;
using ContactsAPI.Application.Helper;
using ContactsAPI.Application.Users.Dtos;
using ContactsAPI.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;

namespace ContactsAPI.Application.Users.Queries.GetAllUsers
{
    public class GetAllUsersHandler
    : IRequestHandler<GetAllUsersQuery, PagedResult<UserDto>>
    {
        private readonly ContactsDbContext _context;

        public GetAllUsersHandler(ContactsDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Users.AsQueryable();

            query = query.ApplySearch(request.Search);

            if (request.Status.HasValue)
                query = query.Where(u => u.Status == request.Status.Value);

            if (request.FromDate.HasValue)
                query = query.Where(u => u.CreatedDate >= request.FromDate.Value.Date);

            if (request.ToDate.HasValue)
                query = query.Where(u => u.CreatedDate < request.ToDate.Value.Date.AddDays(1));

            // ✅ Sort before count — order doesn't affect count but keeps intent clear
            query = query.ApplySorting(request.SortBy, request.SortOrder);

            var totalCount = await query.CountAsync(cancellationToken);

            var users = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var items = users.Select((u, index) => new UserDto
            {
                UserId = u.UserId,
                Sequence = ((request.Page - 1) * request.PageSize) + index + 1,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role,
                Status = u.Status.ToString(),
                CreatedDate = u.CreatedDate
            }).ToList();

            return new PagedResult<UserDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}


