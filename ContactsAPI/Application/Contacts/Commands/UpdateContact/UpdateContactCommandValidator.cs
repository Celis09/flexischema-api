using ContactsAPI.Application.Abstractions;
using ContactsAPI.Application.Contacts.Dtos;
using ContactsAPI.Data;
using ContactsAPI.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Application.Contacts.Commands.UpdateContact
{
    public class UpdateContactCommandValidator : AbstractValidator<UpdateContactCommand>
    {
        public UpdateContactCommandValidator(
            IConfigService configService,
            ContactsDbContext dbContext,
            IValidator<ContactExtraFieldRequest> extraFieldValidator)
        {
            // ── ID ────────────────────────────────────────────────────────────
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Contact ID must be a valid positive number");

            // ── Name ──────────────────────────────────────────────────────────
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Contact name is required")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

            // ── Email ─────────────────────────────────────────────────────────
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Valid email is required")
                .Matches(@"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$")
                .WithMessage("Email must have a valid domain suffix")
                .MustAsync(async (command, email, ct) =>
                    !await dbContext.Contacts.AnyAsync(c => c.Email == email && c.Id != command.Id, ct))
                .WithMessage("Email already exists");

            // ── Per-field validation (existence, active, required value) ──────
            RuleForEach(x => x.ExtraFields)
                .SetValidator(extraFieldValidator);

            // ── Max fields cap ────────────────────────────────────────────────
            RuleFor(x => x.ExtraFields)
                .MustAsync(async (fields, ct) =>
                {
                    var max = await configService.GetIntAsync("MaxExtraFieldsPerContact", ct);
                    return fields == null || fields.Count <= max;
                })
                .WithMessage("Too many extra fields provided");

            // ── Required definitions coverage check ───────────────────────────
            // Ensures that every active required definition either has a non-blank
            // value in the submitted fields OR already has a saved value in the DB
            // (so the user isn't forced to re-submit unchanged required fields).
            RuleFor(x => x)
                .MustAsync(async (parent, command, ctx, ct) =>
                {
                    var requiredDefs = await dbContext.ExtraFieldDefinitions
                        .Where(d => d.IsActive && d.IsRequired)
                        .ToListAsync(ct);

                    if (requiredDefs.Count == 0) return true;

                    var submittedWithValue = (command.ExtraFields ?? [])
                        .Where(ef => !string.IsNullOrWhiteSpace(ef.FieldValue))
                        .Select(ef => ef.ExtraFieldDefinitionId)
                        .ToHashSet();

                    var requiredDefIds = requiredDefs
                        .Select(d => d.ExtraFieldDefinitionId)
                        .ToList();

                    var savedWithValue = await dbContext.ContactExtraFields
                        .Where(ef => ef.ContactId == command.Id
                                  && requiredDefIds.Contains(ef.ExtraFieldDefinitionId)
                                  && ef.FieldValue != null && ef.FieldValue.Trim() != "")
                        .Select(ef => ef.ExtraFieldDefinitionId)
                        .ToListAsync(ct);

                    var coveredIds = submittedWithValue.Concat(savedWithValue).ToHashSet();

                    var missingNames = requiredDefs
                        .Where(d => !coveredIds.Contains(d.ExtraFieldDefinitionId))
                        .Select(d => $"{d.FieldName} ({d.FieldType.ToLabel()})")
                        .ToList();

                    if (missingNames.Count == 0) return true;

                    ctx.MessageFormatter.AppendArgument("MissingFields", string.Join(", ", missingNames));
                    return false;
                })
                .WithMessage("Missing required field(s): {MissingFields}.");
        }
    }
}