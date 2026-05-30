using ContactsAPI.Application.Abstractions;
using ContactsAPI.Application.Contacts.Commands.CreateContact;
using ContactsAPI.Application.Contacts.Dtos;
using ContactsAPI.Data;
using ContactsAPI.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

public class CreateContactCommandValidator : AbstractValidator<CreateContactCommand>
{
    public CreateContactCommandValidator(
        IConfigService configService,
        ContactsDbContext dbContext,
        IValidator<ContactExtraFieldRequest> extraFieldValidator)
    {
        // ── Name ──────────────────────────────────────────────────────────────
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Contact name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        // ── Email ─────────────────────────────────────────────────────────────
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Valid email is required")
            .Matches(@"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$").WithMessage("Email must have a valid domain suffix")
            .MustAsync(async (email, ct) =>
                !await dbContext.Contacts.AnyAsync(c => c.Email == email, ct))
            .WithMessage("Email already exists");

        // ── Per-field validation (existence, active, required value) ──────────
        RuleForEach(x => x.ExtraFields)
            .SetValidator(extraFieldValidator);

        // ── Max fields cap ────────────────────────────────────────────────────
        RuleFor(x => x.ExtraFields)
            .MustAsync(async (fields, ct) =>
            {
                var max = await configService.GetIntAsync("MaxExtraFieldsPerContact", ct);
                return fields == null || fields.Count <= max;
            })
            .WithMessage("Too many extra fields provided");

        // ── Required definitions coverage check ───────────────────────────────
        // The per-field validator above catches required fields that ARE submitted
        // but have a blank value. This rule catches required definitions that are
        // missing from the submitted list entirely.
        RuleFor(x => x.ExtraFields)
            .MustAsync(async (command, fields, ctx, ct) =>
            {
                var requiredDefs = await dbContext.ExtraFieldDefinitions
                    .Where(d => d.IsActive && d.IsRequired)
                    .ToListAsync(ct);

                var submittedIds = (fields ?? [])
                    .Where(ef => !string.IsNullOrWhiteSpace(ef.FieldValue))
                    .Select(ef => ef.ExtraFieldDefinitionId)
                    .ToHashSet();

                var missingNames = requiredDefs
                    .Where(d => !submittedIds.Contains(d.ExtraFieldDefinitionId))
                    .Select(d => $"{d.FieldName} ({d.FieldType.ToLabel()})")
                    .ToList();

                if (missingNames.Count == 0) return true;

                ctx.MessageFormatter.AppendArgument("MissingFields", string.Join(", ", missingNames));
                return false;
            })
            .WithMessage("Missing required field(s): {MissingFields}.");
    }
}