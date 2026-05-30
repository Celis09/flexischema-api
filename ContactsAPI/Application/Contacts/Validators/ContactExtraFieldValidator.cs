using ContactsAPI.Application.Contacts.Dtos;
using ContactsAPI.Application.Helper;
using ContactsAPI.Data;
using ContactsAPI.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ContactsAPI.Application.Contacts.Validators
{
    public partial class ContactExtraFieldValidator : AbstractValidator<ContactExtraFieldRequest>
    {
        [GeneratedRegex(@"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$")]
        public static partial Regex EmailRegex();

        [GeneratedRegex(@"^\+?[0-9]{7,15}$")]
        public static partial Regex PhoneRegex();

        private static readonly string[] DateFormats =
        [
            "MM/dd/yyyy", "M/d/yyyy", "M/dd/yyyy", "MM/d/yyyy",
            "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss"
        ];

        public ContactExtraFieldValidator(ContactsDbContext context, IMemoryCache cache)
        {
            // ── Stateless rules (no DB) ───────────────────────────────────────
            RuleFor(x => x.FieldValue)
                .Must(v => string.IsNullOrWhiteSpace(v) || v.Trim().Length <= 200)
                .WithMessage("Extra field value must not exceed 200 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.FieldValue));

            RuleFor(x => x.ExtraFieldDefinitionId)
                .GreaterThan(0)
                .WithMessage("Extra field definition ID must be valid");

            // ── DB-backed rule (single cached bulk load) ──────────────────────
            RuleFor(x => x)
                .CustomAsync(async (dto, ctx, ct) =>
                {
                    if (dto.ExtraFieldDefinitionId <= 0) return;

                    // Load all active definitions+options once; reuse from cache.
                    var definitions = await cache.GetOrCreateAsync(
                        "extrafield:definitions:withOptions",
                        async entry =>
                        {
                            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                            return await context.ExtraFieldDefinitions
                                .AsNoTracking()
                                .Include(d => d.Options)
                                .Where(d => d.IsActive)
                                .ToDictionaryAsync(d => d.ExtraFieldDefinitionId, ct);
                        });

                    if (definitions == null ||
                        !definitions.TryGetValue(dto.ExtraFieldDefinitionId, out var definition))
                    {
                        ctx.AddFailure(
                            $"Definition {dto.ExtraFieldDefinitionId} not found or is inactive");
                        return;
                    }

                    var fieldName = definition.FieldName;
                    var value = dto.FieldValue?.Trim() ?? string.Empty;
                    var isEmpty = string.IsNullOrWhiteSpace(value);

                    if (definition.IsRequired && isEmpty)
                    {
                        ctx.AddFailure($"'{fieldName}' is required ({definition.FieldType.ToLabel()})");
                        return;
                    }

                    if (isEmpty) return;

                    switch (definition.FieldType)
                    {
                        case ExtraFieldType.Email:
                            if (!EmailRegex().IsMatch(value))
                                ctx.AddFailure(
                                    $"Field '{fieldName}' must be a valid email address");
                            break;

                        case ExtraFieldType.Phone:
                            if (!PhoneRegex().IsMatch(value))
                                ctx.AddFailure(
                                    $"Field '{fieldName}' must be a valid phone number");
                            break;

                        case ExtraFieldType.Url:
                            if (!Uri.TryCreate(value, UriKind.Absolute, out _))
                                ctx.AddFailure(
                                    $"Field '{fieldName}' must be a valid URL");
                            break;

                        case ExtraFieldType.Date:
                            if (!DateTime.TryParseExact(value, DateFormats,
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.None, out var parsedDate))
                            {
                                ctx.AddFailure(
                                    $"Field '{fieldName}' must be a valid date " +
                                    "(e.g. 5/12/2026, 05/12/2026, or 2026-05-12)");
                                break;
                            }

                            var nameLower = fieldName.Trim().ToLower();
                            var isBirthField = nameLower.Contains("birth") ||
                                               nameLower.Contains("birthday");

                            if (parsedDate.Date > PhilippineTime.Now.Date)
                            {
                                var label = isBirthField ? "birthday" : fieldName;
                                ctx.AddFailure($"Field '{label}' cannot be a future date");
                            }
                            break;

                        case ExtraFieldType.Number:
                            if (!decimal.TryParse(value, NumberStyles.Any,
                                    CultureInfo.InvariantCulture, out _))
                                ctx.AddFailure(
                                    $"Field '{fieldName}' must be a valid number");
                            break;

                        case ExtraFieldType.Option:
                            // Options are already loaded via Include — no extra DB call.
                            var validOptions = definition.Options
                                .Select(o => o.OptionValue)
                                .ToList();

                            if (validOptions.Count > 0 &&
                                !validOptions.Any(o => o.Equals(value, StringComparison.OrdinalIgnoreCase)))
                            {
                                ctx.AddFailure(
                                    $"Field '{fieldName}' must be one of: {string.Join(", ", validOptions)}");
                            }
                            break;

                        case ExtraFieldType.Text:
                        default:
                            break;
                    }
                });
        }
    }
}