using ContactsAPI.Services;

namespace ContactsAPI.Application.Helper
{
    public static class MetricsHelper
    {
        public static void RecordValidationValid(int count = 1)
        {
            MetricsRegistry.ValidationValidRows.Add(count);
            MetricsTracker.IncrementValidationValid(count);
        }

        public static void RecordValidationInvalid(int count = 1)
        {
            MetricsRegistry.ValidationInvalidRows.Add(count);
            MetricsTracker.IncrementValidationInvalid(count);
        }

        public static void RecordAuditLog(int count = 1)
        {
            MetricsRegistry.AuditLogs.Add(count);
            MetricsTracker.IncrementAuditLogs(count);
        }

        public static void RecordExceptionHandled(int count = 1)
        {
            MetricsRegistry.ExceptionsHandled.Add(count);
            MetricsTracker.IncrementExceptionsHandled(count);
        }

        public static void RecordExceptionUnhandled(int count = 1)
        {
            MetricsRegistry.ExceptionsUnhandled.Add(count);
            MetricsTracker.IncrementExceptionsUnhandled(count);
        }

        public static void RecordExportSuccess(int count = 1)
        {
            MetricsRegistry.ExportSuccess.Add(count);
            MetricsTracker.IncrementExportSuccess(count);
        }

        public static void RecordExportFailed(int count = 1)
        {
            MetricsRegistry.ExportFailed.Add(count);
            MetricsTracker.IncrementExportFailed(count);
        }
    }
}

