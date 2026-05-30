using ContactsAPI.Application.Helper;
using FluentValidation;

namespace ContactsAPI.Application.Users.Queries.GetAllUsers
{
    public class GetAllUsersQueryValidator : AbstractValidator<GetAllUsersQuery>
    {
        public GetAllUsersQueryValidator()
        {
            // Page must be >= 1
            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page must be at least 1.");

            // PageSize must be between 1 and 100 (adjust as needed)
            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page Size must be between 1 and 100.");

            // SortOrder must be "asc" or "desc"
            RuleFor(x => x.SortOrder)
                .Must(s => string.IsNullOrEmpty(s) || s.Equals("asc", StringComparison.OrdinalIgnoreCase) || s.Equals("desc", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Sort Order must be 'asc' or 'desc'.");

            // ToDate must be >= FromDate
            RuleFor(x => x)
                .Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue || x.ToDate.Value.Date >= x.FromDate.Value.Date)
                .WithMessage("To Date must be greater than or equal to From Date.");

            // Prevent future dates
            RuleFor(x => x.FromDate)
                .Must(d => !d.HasValue || d.Value.Date <= PhilippineTime.Now.Date)
                .WithMessage("From Date cannot be in the future.");

            RuleFor(x => x.ToDate)
                .Must(d => !d.HasValue || d.Value.Date <= PhilippineTime.Now.Date)
                .WithMessage("To Date cannot be in the future.");
        }
    }
}
