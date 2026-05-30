using ContactsAPI.Application.Helper;

namespace ContactsAPI.Models
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }
        public DateTime Timestamp { get; set; } = PhilippineTime.Now;
        // Nullable FK to User
        public int? UserId { get; set; }
        public User? User { get; set; }
        public string PerformedByUsername { get; set; } = "Anonymous";
        public string UserRole { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string RequestData { get; set; } = string.Empty;
        public string ResponseData { get; set; } = string.Empty;
        public bool Success { get; set; } = true;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}