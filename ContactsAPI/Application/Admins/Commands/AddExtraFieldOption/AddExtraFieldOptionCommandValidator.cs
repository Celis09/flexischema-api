using ContactsAPI.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Application.Admins.Commands.AddExtraFieldOption;

public class AddExtraFieldOptionCommandValidator
    : AbstractValidator<AddExtraFieldOptionCommand>
{
    public AddExtraFieldOptionCommandValidator(ContactsDbContext context)
    {
        RuleFor(x => x.OptionValue)
            .NotEmpty().WithMessage("Option value cannot be empty")
            .MaximumLength(100).WithMessage("Option value must not exceed 100 characters");

        RuleFor(x => x.DefinitionId)
            .GreaterThan(0).WithMessage("Definition ID must be valid")
            .MustAsync(async (id, ct) =>
                await context.ExtraFieldDefinitions
                    .AnyAsync(d => d.ExtraFieldDefinitionId == id, ct))
            .WithMessage(x => $"Definition {x.DefinitionId} not found")
            .When(x => x.DefinitionId > 0);

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var normalized = cmd.OptionValue.Trim().ToLower();
                return !await context.ExtraFieldOptions
                    .AnyAsync(o => o.ExtraFieldDefinitionId == cmd.DefinitionId &&
                                   o.OptionValue.ToLower() == normalized, ct);
            })
            .WithMessage(x => $"Option '{x.OptionValue}' already exists for this definition")
            .When(x => x.DefinitionId > 0 && !string.IsNullOrWhiteSpace(x.OptionValue));
    }
}