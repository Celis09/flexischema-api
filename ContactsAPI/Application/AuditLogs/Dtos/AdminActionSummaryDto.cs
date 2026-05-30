namespace ContactsAPI.Application.AuditLogs.Dtos
{
    public class AdminActionSummaryDto
    {
        public string ActionType { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
