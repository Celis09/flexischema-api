using ContactsAPI.Application.Admins.Commands.AddExtraFieldDefinition;
using ContactsAPI.Entities;
using ContactsAPI.Models;
using Xunit;

namespace ContactsAPI.Test.Validators.AdminValidators;

public class AddExtraFieldDefinitionCommandValidatorTests
{
    private AddExtraFieldDefinitionCommandValidator BuildValidator(string dbName)
    {
        var ctx = ValidatorFixture.CreateContext(dbName);
        return new AddExtraFieldDefinitionCommandValidator(ctx);
    }

    // ── FieldName ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_EmptyFieldName_Fails()
    {
        var validator = BuildValidator(nameof(Validate_EmptyFieldName_Fails));
        var result = await validator.ValidateAsync(new AddExtraFieldDefinitionCommand
        {
            FieldName = "",
            FieldType = ExtraFieldType.Text
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Field name is required");
    }

    [Fact]
    public async Task Validate_FieldNameTooLong_Fails()
    {
        var validator = BuildValidator(nameof(Validate_FieldNameTooLong_Fails));
        var result = await validator.ValidateAsync(new AddExtraFieldDefinitionCommand
        {
            FieldName = new string('A', 51),
            FieldType = ExtraFieldType.Text
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == "Field name must not exceed 50 characters");
    }

    [Fact]
    public async Task Validate_DuplicateFieldName_Fails()
    {
        await using var ctx = ValidatorFixture.CreateContext(
            nameof(Validate_DuplicateFieldName_Fails));

        ctx.ExtraFieldDefinitions.Add(new ExtraFieldDefinition
        {
            FieldName = "Twitter",
            FieldType = ExtraFieldType.Text
        });
        await ctx.SaveChangesAsync();

        var validator = new AddExtraFieldDefinitionCommandValidator(ctx);
        var result = await validator.ValidateAsync(new AddExtraFieldDefinitionCommand
        {
            FieldName = "Twitter",
            FieldType = ExtraFieldType.Text
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == "A field with this name already exists");
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_UniqueValidName_Passes()
    {
        var validator = BuildValidator(nameof(Validate_UniqueValidName_Passes));
        var result = await validator.ValidateAsync(new AddExtraFieldDefinitionCommand
        {
            FieldName = "LinkedIn",
            FieldType = ExtraFieldType.Url,
            IsActive = true
        });

        Assert.True(result.IsValid);
    }
}