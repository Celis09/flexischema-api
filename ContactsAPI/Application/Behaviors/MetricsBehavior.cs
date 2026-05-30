using ContactsAPI.Application.Admins.Commands.AddExtraFieldDefinition;
using ContactsAPI.Application.Admins.Commands.ImportContacts;
using ContactsAPI.Application.Contacts.Queries.ExportContacts;
using ContactsAPI.Application.Helper;
using ContactsAPI.Services;
using MediatR;
using System.Diagnostics;

namespace ContactsAPI.Application.Behaviors
{
    public class MetricsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var response = await next();
                sw.Stop();

                // Always record latency
                MetricsRegistry.RequestLatency.Record(sw.Elapsed.TotalSeconds);

                // Request succeeded → handled
                MetricsHelper.RecordExceptionHandled();

                // Classify by request type
                switch (request)
                {
                    case ImportContactsCommand importCmd:
                        if (response is ImportResult importResult)
                        {
                            if (importResult.ImportedCount > 0)
                                MetricsHelper.RecordValidationValid(importResult.ImportedCount);

                            if (importResult.FailedRows?.Count > 0)
                                MetricsHelper.RecordValidationInvalid(importResult.FailedRows.Count);
                        }
                        break;

                    case ExportContactsQuery exportCmd:
                        if (response is ExportResult exportResult)
                        {
                            if (!string.IsNullOrEmpty(exportResult.Content))
                                MetricsHelper.RecordExportSuccess();
                            else
                                MetricsHelper.RecordExportFailed();
                        }
                        break;

                    case AddExtraFieldDefinitionCommand:
                        MetricsHelper.RecordAuditLog();
                        break;
                }

                return response;
            }
            catch (Exception)
            {
                sw.Stop();

                // Record latency even for failed requests
                MetricsRegistry.RequestLatency.Record(sw.Elapsed.TotalSeconds);

                // Request failed → unhandled exception
                MetricsHelper.RecordExceptionUnhandled();

                throw;
            }
        }
    }
}


