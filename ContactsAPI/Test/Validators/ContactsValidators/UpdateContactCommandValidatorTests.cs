using ContactsAPI.Application.Abstractions;
using ContactsAPI.Application.Contacts.Commands.UpdateContact;
using ContactsAPI.Application.Contacts.Dtos;
using ContactsAPI.Data;
using ContactsAPI.Entities;
using ContactsAPI.Models;
using FluentValidation;
using Xunit;

namespace ContactsAPI.Test.Validators.ContactsValidators;

public class UpdateContactCommandValidatorTests
{
    private static UpdateContactCommandValidator BuildValidator(
        ContactsDbContext context,
        IConfigService? config = null,
        IValidator<ContactExtraFieldRequest>? fieldValidator = null) =>
        new(
            config ?? ValidatorFixture.MockConfig(),
            context,
            fieldValidator ?? ValidatorFixture.PassthroughFieldValidator()
        );

    // ── ID rule ───────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Validate_InvalidId_FailsWithIdMessage(int id)
    {
        await using var ctx = ValidatorFixture.CreateContext($"{nameof(Validate_InvalidId_FailsWithIdMessage)}_{id}");
        var validator = BuildValidator(ctx);

        var result = await validator.ValidateAsync(new UpdateContactCommand
        {
            Id = id,
            Name = "Someone",
            Email = "someone@example.com"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("valid positive number"));
    }

    // ── Name rule ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_EmptyName_FailsWithNameRequiredMessage()
    {
        await using var ctx = ValidatorFixture.CreateContext(nameof(Validate_EmptyName_FailsWithNameRequiredMessage));
        var validator = BuildValidator(ctx);

        var result = await validator.ValidateAsync(new UpdateContactCommand
        {
            Id = 1,
            Name = "",
            Email = "test@example.com"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Contact name is required");
    }

    // ── Email uniqueness (excluding self) ─────────────────────────────────────

    [Fact]
    public async Task Validate_EmailTakenByAnotherContact_FailsWithEmailExistsMessage()
    {
        await using var ctx = ValidatorFixture.CreateContext(nameof(Validate_EmailTakenByAnotherContact_FailsWithEmailExistsMessage));

        ctx.Contacts.AddRange(
            new Contact { Name = "Owner", Email = "taken@example.com" },
            new Contact { Name = "Updater", Email = "updater@example.com" }
        );
        await ctx.SaveChangesAsync();

        var contacts = ctx.Contacts.ToList();
        var owner = contacts.First(c => c.Email == "taken@example.com");
        var updater = contacts.First(c => c.Email == "updater@example.com");

        var validator = BuildValidator(ctx);

        var result = await validator.ValidateAsync(new UpdateContactCommand
        {
            Id = updater.Id,
            Name = "Updater",
            Email = owner.Email    // trying to steal another contact's email
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Email already exists");
    }

    [Fact]
    public async Task Validate_SameEmailAsOwnContact_PassesEmailUniquenessRule()
    {
        await using var ctx = ValidatorFixture.CreateContext(nameof(Validate_SameEmailAsOwnContact_PassesEmailUniquenessRule));

        var contact = new Contact { Name = "Self", Email = "self@example.com" };
        ctx.Contacts.Add(contact);
        await ctx.SaveChangesAsync();

        var validator = BuildValidator(ctx);

        var result = await validator.ValidateAsync(new UpdateContactCommand
        {
            Id = contact.Id,
            Name = "Self Updated",
            Email = "self@example.com",   // same email — should be allowed
            ExtraFields = []
        });

        Assert.DoesNotContain(result.Errors, e => e.ErrorMessage == "Email already exists");
    }

    // ── Required field coverage ───────────────────────────────────────────────

    [Fact]
    public async Task Validate_RequiredFieldMissingAndNotSaved_FailsWithMissingFieldsMessage()
    {
        await using var ctx = ValidatorFixture.CreateContext(nameof(Validate_RequiredFieldMissingAndNotSaved_FailsWithMissingFieldsMessage));

        ctx.ExtraFieldDefinitions.Add(new ExtraFieldDefinition
        {
            FieldName = "LinkedIn",
            FieldType = ExtraFieldType.Url,
            IsActive = true,
            IsRequired = true
        });
        var contact = new Contact { Name = "Dave", Email = "dave@example.com" };
        ctx.Contacts.Add(contact);
        await ctx.SaveChangesAsync();

        var validator = BuildValidator(ctx);

        var result = await validator.ValidateAsync(new UpdateContactCommand
        {
            Id = contact.Id,
            Name = "Dave",
            Email = "dave@example.com",
            ExtraFields = [] // LinkedIn not submitted, not saved
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Missing required field"));
    }

    [Fact]
    public async Task Validate_RequiredFieldCoveredBySavedValue_PassesRequiredCheck()
    {
        await using var ctx = ValidatorFixture.CreateContext(nameof(Validate_RequiredFieldCoveredBySavedValue_PassesRequiredCheck));

        var def = new ExtraFieldDefinition
        {
            FieldName = "LinkedIn",
            FieldType = ExtraFieldType.Url,
            IsActive = true,
            IsRequired = true
        };
        ctx.ExtraFieldDefinitions.Add(def);
        await ctx.SaveChangesAsync();

        var contact = new Contact
        {
            Name = "Eve",
            Email = "eve@example.com",
            ExtraFields =
            [
                new() { ExtraFieldDefinitionId = def.ExtraFieldDefinitionId, FieldValue = "linkedin.com/in/eve" }
            ]
        };
        ctx.Contacts.Add(contact);
        await ctx.SaveChangesAsync();

        var validator = BuildValidator(ctx);

        // LinkedIn is not re-submitted — but it already has a saved value, so it should pass
        var result = await validator.ValidateAsync(new UpdateContactCommand
        {
            Id = contact.Id,
            Name = "Eve Updated",
            Email = "eve@example.com",
            ExtraFields = []
        });

        Assert.DoesNotContain(result.Errors, e => e.ErrorMessage.Contains("Missing required field"));
    }
}