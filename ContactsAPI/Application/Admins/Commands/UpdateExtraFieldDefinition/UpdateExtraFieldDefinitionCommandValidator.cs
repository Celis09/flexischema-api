using ContactsAPI.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Application.Admins.Commands.UpdateExtraFieldDefinition
{
    public class UpdateExtraFieldDefinitionCommandValidator : AbstractValidator<UpdateExtraFieldDefinitionCommand>
    {
        private readonly ContactsDbContext _context;

        public UpdateExtraFieldDefinitionCommandValidator(ContactsDbContext context)
        {
            _context = context;

            RuleFor(x => x.ExtraFieldDefinitionId)
                .GreaterThan(0).WithMessage("Extra field definition ID must be a valid positive number");

            RuleFor(x => x.FieldName)
                .NotEmpty().WithMessage("Field name is required")
                .MaximumLength(50).WithMessage("Field name must not exceed 50 characters")
                .MustAsync(BeUniqueName).WithMessage("A field with this name already exists");

            RuleFor(x => x.FieldType)
                .IsInEnum()
                .WithMessage("Field type must be Text, Email, Option, Number, Phone, Date, or Url");
        }

        private async Task<bool> BeUniqueName(
            UpdateExtraFieldDefinitionCommand command,
            string fieldName,
            CancellationToken cancellationToken)
        {
            var normalizedName = fieldName.ToLower();

            return !await _context.ExtraFieldDefinitions
                .AnyAsync(x => x.FieldName.ToLower() == normalizedName
                            && x.ExtraFieldDefinitionId != command.ExtraFieldDefinitionId, cancellationToken);
        }
    }
}