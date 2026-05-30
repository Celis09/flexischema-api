using ContactsAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Application.Helper
{
    public static class ContactQueryExtensions
    {
        public static IQueryable<Contact> ApplySearch(this IQueryable<Contact> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search)) return query;

            return query.Where(c =>
                EF.Functions.Like(c.Name, $"%{search}%") ||
                (c.Email != null && EF.Functions.Like(c.Email, $"%{search}%")) ||
                c.ExtraFields.Any(f =>
                    EF.Functions.Like(f.FieldValue, $"%{search}%") ||
                    EF.Functions.Like(f.Definition.FieldName, $"%{search}%"))
            );
        }
    }
}
