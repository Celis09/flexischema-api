using ContactsAPI.Application.Helper;
using FluentValidation;

namespace ContactsAPI.Application.AuditLogs.Queries.GetAdminActionsSummary
{
    public class GetAdminActionsSummaryFilterValidator : AbstractValidator<AdminActionSummaryFilter>
    {
        public GetAdminActionsSummaryFilterValidator()
        {
            RuleFor(f => f.FromDate)
                .Must(d => d == null || d <= PhilippineTime.Now)
                .WithMessage("From Date cannot be in the future");

            RuleFor(f => f.ToDate)
                .Must(d => d == null || d <= PhilippineTime.Now)
                .WithMessage("To Date cannot be in the future");

            RuleFor(f => f)
                .Must(f => !(f.FromDate.HasValue && f.ToDate.HasValue) || f.ToDate >= f.FromDate)
                .WithMessage("To Date must be greater than or equal to FromDate");

            RuleFor(f => f.Page)
                .GreaterThan(0).WithMessage("Page must be greater than 0");

            RuleFor(f => f.PageSize)
                .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");
        }
    }
}
