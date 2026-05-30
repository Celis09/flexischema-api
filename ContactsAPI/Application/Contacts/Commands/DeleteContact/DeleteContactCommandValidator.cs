using FluentValidation;

namespace ContactsAPI.Application.Contacts.Commands.DeleteContact
{
    public class DeleteContactCommandValidator : AbstractValidator<DeleteContactCommand>
    {
        public DeleteContactCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Contact ID must be a valid positive number");
        }
    }
}
