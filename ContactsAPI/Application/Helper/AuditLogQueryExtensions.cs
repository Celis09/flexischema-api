using ContactsAPI.Application.AuditLogs.Queries.GetAllAuditLogs;
using ContactsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Application.Helper
{
    public static class AuditLogQueryExtensions
    {
        public static IQueryable<AuditLog> ApplySearch(this IQueryable<AuditLog> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search)) return query;
            return query.Where(a =>
                EF.Functions.Like(a.ActionType, $"%{search}%") ||
                EF.Functions.Like(a.EntityName, $"%{search}%") ||
                EF.Functions.Like(a.EntityId, $"%{search}%") ||
                EF.Functions.Like(a.UserRole, $"%{search}%") ||
                EF.Functions.Like(a.ErrorMessage, $"%{search}%") ||
                EF.Functions.Like(a.PerformedByUsername, $"%{search}%")  // ← FIXED: was a.User.Username (missed Anonymous/Viewer)
            );
        }

        public static IQueryable<AuditLog> ApplyAuditLogFilters(this IQueryable<AuditLog> query, GetAllAuditLogsQuery request)
        {
            if (!string.IsNullOrEmpty(request.ActionType))
                query = query.Where(a => a.ActionType == request.ActionType);

            if (!string.IsNullOrEmpty(request.EntityName))
                query = query.Where(a => a.EntityName == request.EntityName);

            if (!string.IsNullOrEmpty(request.UserRole))
                query = query.Where(a => a.UserRole == request.UserRole);

            if (request.Success.HasValue)
                query = query.Where(a => a.Success == request.Success.Value);

            if (request.UserId.HasValue)
                query = query.Where(a => a.UserId == request.UserId.Value);

            if (!string.IsNullOrEmpty(request.Username))
                query = query.Where(a => a.PerformedByUsername == request.Username);  

            return query;
        }
    }
}