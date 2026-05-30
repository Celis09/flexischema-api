using System.Diagnostics.Metrics;

namespace ContactsAPI.Services
{
    public static class MetricsRegistry
    {
        private static readonly Meter _meter = new("ContactsAPI.Metrics", "1.0");

        public static readonly Counter<int> ValidationValidRows =
            _meter.CreateCounter<int>("validation_valid_total");

        public static readonly Counter<int> ValidationInvalidRows =
            _meter.CreateCounter<int>("validation_invalid_total");

        public static readonly Counter<int> AuditLogs =
            _meter.CreateCounter<int>("audit_logs_total");

        public static readonly Counter<int> ExceptionsHandled =
            _meter.CreateCounter<int>("exceptions_handled_total");

        public static readonly Counter<int> ExceptionsUnhandled =
            _meter.CreateCounter<int>("exceptions_unhandled_total");

        public static readonly Histogram<double> RequestLatency =
            _meter.CreateHistogram<double>("http_request_duration_seconds");

        public static readonly Counter<int> ExportSuccess =
            _meter.CreateCounter<int>("export_success_total");

        public static readonly Counter<int> ExportFailed =
            _meter.CreateCounter<int>("export_failed_total");
    }
}
