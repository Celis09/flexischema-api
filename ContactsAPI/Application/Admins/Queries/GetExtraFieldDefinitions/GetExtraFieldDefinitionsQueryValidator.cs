using FluentValidation;

namespace ContactsAPI.Application.Admins.Queries.GetExtraFieldDefinitions
{
    public class GetExtraFieldDefinitionsQueryValidator : AbstractValidator<GetExtraFieldDefinitionsQuery>
    {
        public GetExtraFieldDefinitionsQueryValidator()
        {
            // RoleFilter must be either Admin or Editor if provided
            RuleFor(x => x.RoleFilter)
                .Must(role => string.IsNullOrEmpty(role) || new[] { "Admin", "Editor" }.Contains(role))
                .WithMessage("Role filter must be Admin or Editor");

            // IsActive is optional, no strict validation needed
            // but you could enforce consistency if required
        }
    }
}
