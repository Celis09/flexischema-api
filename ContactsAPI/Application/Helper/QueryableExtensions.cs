using ContactsAPI.Models;
using System.Linq.Expressions;

namespace ContactsAPI.Application.Extensions
{
    public static class QueryableExtensions
    {
        private static IQueryable<T> Sort<T, TKey>(
            IQueryable<T> query,
            Expression<Func<T, TKey>> keySelector,
            bool isDesc) =>
            isDesc ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);

        // ── User ──────────────────────────────────────────────────────────────────

        public static IQueryable<User> ApplySorting(
            this IQueryable<User> query,
            string? sortBy,
            string? sortOrder)
        {
            var isDesc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
            return sortBy?.ToLower() switch
            {
                "username" => Sort(query, u => u.Username, isDesc),
                "email" => Sort(query, u => u.Email, isDesc),
                "role" => Sort(query, u => u.Role, isDesc),
                "status" => Sort(query, u => u.Status, isDesc),
                "createddate" => Sort(query, u => u.CreatedDate, isDesc),
                _ => Sort(query, u => u.UserId, isDesc),
            };
        }

        // ── AuditLog ──────────────────────────────────────────────────────────────

        public static IQueryable<AuditLog> ApplySorting(
            this IQueryable<AuditLog> query,
            string? sortBy,
            string? sortOrder)
        {
            var isDesc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
            return sortBy?.ToLower() switch
            {
                "timestamp" => Sort(query, a => a.Timestamp, isDesc),
                "username" => Sort(query, a => a.User!.Username, isDesc),
                "userrole" => Sort(query, a => a.UserRole, isDesc),
                "actiontype" => Sort(query, a => a.ActionType, isDesc),
                "entityname" => Sort(query, a => a.EntityName, isDesc),
                "entityid" => Sort(query, a => a.EntityId, isDesc),
                "success" => Sort(query, a => a.Success, isDesc),
                _ => Sort(query, a => a.Timestamp, isDesc), // default: newest first
            };
        }
    }
}
