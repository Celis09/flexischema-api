namespace ContactsAPI.Services
{
    public static class MetricsTracker
    {
        public static int ValidationValidTotal { get; private set; }
        public static int ValidationInvalidTotal { get; private set; }
        public static int AuditLogsTotal { get; private set; }
        public static int ExceptionsHandledTotal { get; private set; }
        public static int ExceptionsUnhandledTotal { get; private set; }
        public static int ExportSuccessTotal { get; private set; }
        public static int ExportFailedTotal { get; private set; }

        public static void IncrementValidationValid(int count = 1)
            => ValidationValidTotal += count;

        public static void IncrementValidationInvalid(int count = 1)
            => ValidationInvalidTotal += count;

        public static void IncrementAuditLogs(int count = 1)
            => AuditLogsTotal += count;

        public static void IncrementExceptionsHandled(int count = 1)
            => ExceptionsHandledTotal += count;

        public static void IncrementExceptionsUnhandled(int count = 1)
            => ExceptionsUnhandledTotal += count;

        public static void IncrementExportSuccess(int count = 1)
            => ExportSuccessTotal += count;

        public static void IncrementExportFailed(int count = 1)
            => ExportFailedTotal += count;
    }
}
