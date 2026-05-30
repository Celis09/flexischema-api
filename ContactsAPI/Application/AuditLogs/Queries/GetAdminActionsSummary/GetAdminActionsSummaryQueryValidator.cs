using ContactsAPI.Application.AuditLogs.Queries.GetAdminActionsSummary;
using FluentValidation;

public class GetAdminActionsSummaryQueryValidator
    : AbstractValidator<GetAdminActionsSummaryQuery>
{
    public GetAdminActionsSummaryQueryValidator()
    {
        RuleFor(q => q.Filter)
            .SetValidator(new GetAdminActionsSummaryFilterValidator());
    }
}
