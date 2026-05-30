using FluentValidation;

namespace ContactsAPI.Application.Admins.Commands.UpdateAdminConfig
{
    public class UpdateAdminConfigCommandValidator : AbstractValidator<UpdateAdminConfigCommand>
    {
        public UpdateAdminConfigCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Config ID must be valid");

            RuleFor(x => x.Key)
                .NotEmpty().WithMessage("Config key is required");

            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("Config value is required");

            RuleFor(x => x.Value)
                .Must(value =>
                {
                    if (int.TryParse(value, out var parsed))
                        return parsed <= AdminConfigConstants.AbsoluteMaxLimit;
                    return false;
                })
                .When(x => x.Key == AdminConfigConstants.LimitConfigKey)
                .WithMessage($"{AdminConfigConstants.LimitConfigKey} cannot exceed {AdminConfigConstants.AbsoluteMaxLimit}");
        }
    }
}