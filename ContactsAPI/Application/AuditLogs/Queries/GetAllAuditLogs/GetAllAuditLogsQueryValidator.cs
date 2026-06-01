using ContactsAPI.Application.Helper;
using FluentValidation;

namespace ContactsAPI.Application.AuditLogs.Queries.GetAllAuditLogs
{
    public class GetAllAuditLogsQueryValidator : AbstractValidator<GetAllAuditLogsQuery>
    {
        public GetAllAuditLogsQueryValidator()
        {
            RuleFor(q => q.FromDate)
                .Must(d => d == null || d <= PhilippineTime.Now)
                .WithMessage("From Date cannot be in the future");

            RuleFor(q => q.ToDate)
                .Must(d => d == null || d <= PhilippineTime.Now)
                .WithMessage("To Date cannot be in the future");

            RuleFor(q => q)
                .Must(q => !(q.FromDate.HasValue && q.ToDate.HasValue) || q.ToDate >= q.FromDate)
                .WithMessage("To Date must be greater than or equal to From Date");

            RuleFor(q => q.Page)
                .GreaterThan(0).WithMessage("Page must be greater than 0");

            RuleFor(q => q.PageSize)
                .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");
        }
    }
}
