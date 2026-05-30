using FluentValidation;

namespace ContactsAPI.Application.Users.Commands.UpdateUser
{
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        private static readonly string[] ValidRoles = ["Admin", "Editor"];

        public UpdateUserCommandValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("User ID must be valid");
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .MaximumLength(50);
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")
                .WithMessage("Valid email is required");
            RuleFor(x => x.Password)
                .Must(password =>
                    !string.IsNullOrEmpty(password) &&
                    password.Length >= 8 &&
                    password.Length <= 128 &&
                    password.Any(char.IsUpper) &&
                    password.Any(char.IsLower) &&
                    password.Any(char.IsDigit) &&
                    password.Any(c => !char.IsLetterOrDigit(c)))
                .When(x => !string.IsNullOrEmpty(x.Password))  // only validate if provided
                .WithMessage("Password must be 8–128 characters and include uppercase, lowercase, a number, and a special character");
            RuleFor(x => x.Role)
                .Must(role => ValidRoles.Contains(role))
                .WithMessage("Role must be Admin or Editor");
        }
    }
}
