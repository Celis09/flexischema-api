using ContactsAPI.Application.Abstractions;
using ContactsAPI.Application.Contacts.Commands.CreateContact;
using ContactsAPI.Application.Contacts.Dtos;
using ContactsAPI.Data;
using ContactsAPI.Entities;
using ContactsAPI.Models;
using FluentValidation;
using Xunit;

namespace ContactsAPI.Test.Validators.ContactsValidators;

public class CreateContactCommandValidatorTests
{
    private CreateContactCommandValidator BuildValidator(
        ContactsDbContext context,
        IConfigService? config = null,
        IValidator<ContactExtraFieldRequest>? fieldValidator = null) =>
        new(
            config ?? ValidatorFixture.MockConfig(),
            context,
            fieldValidator ?? ValidatorFixture.PassthroughFieldValidator()
        );

    // ── Name rules ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_EmptyName_FailsWithNameRequiredMessage()
    {
        await using var ctx = ValidatorFixture.CreateContext(nameof(Validate_EmptyName_FailsWithNameRequiredMessage));
        var validator = BuildValidator(ctx);

        var result = await validator.ValidateAsync(new CreateContactCommand
        {
            Name = "",
            Email = "test@example.com"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Contact name is required");
    }

    [Fact]
    public async Task Validate_NameExceeds100Chars_FailsWithLengthMessage()
    {
        await using var ctx = ValidatorFixture.CreateContext(nameof(Validate_NameExceeds100Chars_FailsWithLengthMessage));
        var validator = BuildValidator(ctx);

        var result = await validator.ValidateAsync(new CreateContactCommand
        {
            Name = new string('A', 101),
            Email = "test@example.com"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Name must not exceed 100 characters");
    }

    // ── Email rules ───────────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyOrNullEmail_FailsWithEmailRequiredMessage(string? email)
    {
        await using var ctx = ValidatorFixture.CreateContext($"{nameof(Validate_EmptyOrNullEmail_FailsWithEmailRequiredMessage)}_{email}");
        var validator = BuildValidator(ctx);

        var result = await validator.ValidateAsync(new CreateContactCommand
        {
            Name = "Alice",
            Email = email
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Email is required");
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing@domain")]
    [InlineData("@nodomain.com")]
    public async Task Validate_InvalidEmailFormat_FailsWithEmailFormatMessage(string email)
    {
        await using var ctx = ValidatorFixture.CreateContext($"{nameof(Validate_InvalidEmailFormat_FailsWithEmailFormatMessage)}_{email}");
        var validator = BuildValidator(ctx);

        var result = await validator.ValidateAsync(new CreateContactCommand
        {
            Name = "Alice",
            Email = email
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.ErrorMessage.Contains("email") || e.ErrorMessage.Contains("domain"));
    }

    [Fact]
    public async Task Validate_DuplicateEmail_FailsWithEmailExistsMessage()
    {
        await using var ctx = ValidatorFixture.CreateContext(nameof(Validate_DuplicateEmail_FailsWithEmailExistsMessage));

        ctx.Contacts.Add(new Contact { Name = "Existing", Email = "taken@example.com" });
        await ctx.SaveChangesAsync();

        var validator = BuildValidator(ctx);

        var result = await validator.ValidateAsync(new CreateContactCommand
        {
            Name = "New Person",
            Email = "taken@example.com"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Email already exists");
    }

    // ── Extra-field rules ─────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_ExtraFieldsExceedMaxCap_FailsWithTooManyFieldsMessage()
    {
        await using var ctx = ValidatorFixture.CreateContext(nameof(Validate_ExtraFieldsExceedMaxCap_FailsWithTooManyFieldsMessage));
        var validator = BuildValidator(ctx, config: ValidatorFixture.MockConfig(maxExtraFields: 2));

        var result = await validator.ValidateAsync(new CreateContactCommand
        {
            Name = "Bob",
            Email = "bob@example.com",
            ExtraFields = new List<ContactExtraFieldRequest>
            {
                new() { ExtraFieldDefinitionId = 1, FieldValue = "A" },
                new() { ExtraFieldDefinitionId = 2, FieldValue = "B" },
                new() { ExtraFieldDefinitionId = 3, FieldValue = "C" }   // exceeds cap of 2
            }
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Too many extra fields provided");
    }

    [Fact]
    public async Task Validate_MissingRequiredFieldDefinition_FailsWithMissingFieldsMessage()
    {
        await using var ctx = ValidatorFixture.CreateContext(nameof(Validate_MissingRequiredFieldDefinition_FailsWithMissingFieldsMessage));

        ctx.ExtraFieldDefinitions.Add(new ExtraFieldDefinition
        {
            FieldName = "Phone",
            FieldType = ExtraFieldType.Phone,
            IsActive = true,
            IsRequired = true
        });
        await ctx.SaveChangesAsync();

        var validator = BuildValidator(ctx);

        var result = await validator.ValidateAsync(new CreateContactCommand
        {
            Name = "Carol",
            Email = "carol@example.com",
            ExtraFields = new List<ContactExtraFieldRequest>() // Phone is missing
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Missing required field"));
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_ValidCommand_PassesAllRules()
    {
        await using var ctx = ValidatorFixture.CreateContext(nameof(Validate_ValidCommand_PassesAllRules));
        var validator = BuildValidator(ctx);

        var result = await validator.ValidateAsync(new CreateContactCommand
        {
            Name = "Valid Person",
            Email = "valid@example.com",
            ExtraFields = new List<ContactExtraFieldRequest>()
        });

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
