using ContactsAPI.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Application.Admins.Commands.AddExtraFieldDefinition
{
    public class AddExtraFieldDefinitionCommandValidator : AbstractValidator<AddExtraFieldDefinitionCommand>
    {
        private readonly ContactsDbContext _context;

        public AddExtraFieldDefinitionCommandValidator(ContactsDbContext context)
        {
            _context = context;

            RuleFor(x => x.FieldName)
                .NotEmpty().WithMessage("Field name is required")
                .MaximumLength(50).WithMessage("Field name must not exceed 50 characters")
                .MustAsync(BeUniqueName).WithMessage("A field with this name already exists");

            RuleFor(x => x.FieldType)
                .IsInEnum()
                .WithMessage("Field type must be Text, Email, Option, Number, Phone, Date, or Url");
        }

        private async Task<bool> BeUniqueName(string fieldName, CancellationToken cancellationToken)
        {
            return !await _context.ExtraFieldDefinitions
                .AnyAsync(x => x.FieldName.ToLower() == fieldName.ToLower(), cancellationToken);
        }
    }
}