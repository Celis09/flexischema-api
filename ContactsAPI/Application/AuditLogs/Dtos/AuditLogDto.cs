namespace ContactsAPI.Application.AuditLogs.Dtos
{
    public class AuditLogDto
    {
        public int AuditLogId { get; set; }
        public DateTime Timestamp { get; set; }
        public int? UserId { get; set; }
        public string PerformedByUsername { get; set; } = "Anonymous";
        public string UserRole { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}