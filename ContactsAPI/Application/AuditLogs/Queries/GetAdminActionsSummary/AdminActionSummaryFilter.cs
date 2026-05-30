namespace ContactsAPI.Application.AuditLogs.Queries.GetAdminActionsSummary
{
    public class AdminActionSummaryFilter
    {
        public string? Role { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Pagination
        public int Page { get; set; } = 1;       // default: first page
        public int PageSize { get; set; } = 20;  // default: 20 results per page

        // Sorting
        public string SortBy { get; set; } = "Count";   // default sort column
        public string SortOrder { get; set; } = "desc"; // "asc" or "desc"
        public bool UseCalendarDay { get; set; } = false;
    }
}
