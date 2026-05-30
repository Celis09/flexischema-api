using System;
using System.Linq;
using ContactsAPI.Entities;

namespace ContactsAPI.Application.Extensions
{
    public static class ContactQueryableExtensions
    {
        public static IQueryable<Contact> ApplySorting(
            this IQueryable<Contact> query,
            string? sortBy,
            string? sortOrder)
        {
            var isDesc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

            // 1. Evaluate primary sort column (now including "id")
            IOrderedQueryable<Contact> orderedQuery = sortBy?.ToLower() switch
            {
                "id" => isDesc ? query.OrderByDescending(c => c.Id) : query.OrderBy(c => c.Id),
                "name" => isDesc ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
                "email" => isDesc ? query.OrderByDescending(c => c.Email) : query.OrderBy(c => c.Email),
                "status" => isDesc ? query.OrderByDescending(c => c.Status) : query.OrderBy(c => c.Status),
                "createddate" => isDesc ? query.OrderByDescending(c => c.CreatedDate) : query.OrderBy(c => c.CreatedDate),

                // Dynamically route ExtraField sorting (e.g. extra_birthday, extra_country)
                _ when sortBy != null && sortBy.StartsWith("extra_", StringComparison.OrdinalIgnoreCase) =>
                    SortByExtraField(query, sortBy["extra_".Length..], isDesc),

                // Fallback default sort
                _ => query.OrderBy(c => c.Name)
            };

            // 2. Continuous deterministic tie-breaker so pagination never skips rows
            return orderedQuery.ThenBy(c => c.Id);
        }

        private static IOrderedQueryable<Contact> SortByExtraField(
            IQueryable<Contact> query,
            string fieldName,
            bool isDesc)
        {
            var normalizedFieldName = fieldName.ToLower();

            // Safe EF Core subquery that preserves your Handler's .Include() chains
            if (isDesc)
            {
                return query.OrderByDescending(c => c.ExtraFields
                    .Where(f => f.Definition.IsActive && f.Definition.FieldName.ToLower() == normalizedFieldName)
                    .OrderBy(f => f.ExtraFieldId)
                    .Select(f => f.FieldValue ?? "")
                    .FirstOrDefault());
            }

            return query.OrderBy(c => c.ExtraFields
                .Where(f => f.Definition.IsActive && f.Definition.FieldName.ToLower() == normalizedFieldName)
                .OrderBy(f => f.ExtraFieldId)
                .Select(f => f.FieldValue ?? "")
                .FirstOrDefault());
        }
    }
}