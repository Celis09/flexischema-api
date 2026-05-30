using ContactsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Application.Helper
{
    public static class UserQueryExtensions
    {
        public static IQueryable<User> ApplySearch(this IQueryable<User> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search)) return query;

            return query.Where(u =>
                EF.Functions.Like(u.Username, $"%{search}%") ||
                EF.Functions.Like(u.Email, $"%{search}%") ||
                EF.Functions.Like(u.Role, $"%{search}%")
            );
        }
    }
}
