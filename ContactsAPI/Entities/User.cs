using ContactsAPI.Application.Helper;

namespace ContactsAPI.Models
{
    public enum UserStatus
    {
        Active,
        Inactive,
        Suspended
    }
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "Editor"; // Admin or Editor

        // New fields
        public UserStatus Status { get; set; } = UserStatus.Active;
        public DateTime CreatedDate { get; set; } = PhilippineTime.Now;

        // Navigation
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
