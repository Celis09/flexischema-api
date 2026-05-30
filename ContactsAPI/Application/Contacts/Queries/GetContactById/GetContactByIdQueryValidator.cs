using FluentValidation;

namespace ContactsAPI.Application.Contacts.Queries.GetContactById
{
    public class GetContactByIdQueryValidator : AbstractValidator<GetContactByIdQuery>
    {
        public GetContactByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Contact ID must be a valid positive number");
        }
    }
}
