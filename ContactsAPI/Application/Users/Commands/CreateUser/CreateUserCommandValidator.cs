using FluentValidation;

namespace ContactsAPI.Application.Users.Commands.CreateUser
{
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        private static readonly string[] ValidRoles = ["Admin", "Editor"];

        public CreateUserCommandValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .MaximumLength(50);
            RuleFor(x => x.Email)
                .NotEmpty()
                .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")
                .WithMessage("Valid email is required");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .Must(password =>
                    !string.IsNullOrEmpty(password) &&
                    password.Length >= 8 &&
                    password.Length <= 128 &&
                    password.Any(char.IsUpper) &&
                    password.Any(char.IsLower) &&
                    password.Any(char.IsDigit) &&
                    password.Any(c => !char.IsLetterOrDigit(c)))
                .WithMessage("Password must be 8–128 characters and include uppercase, lowercase, a number, and a special character");
            RuleFor(x => x.Role)
                .Must(role => ValidRoles.Contains(role))
                .WithMessage("Role must be Admin or Editor");
        }
    }
}