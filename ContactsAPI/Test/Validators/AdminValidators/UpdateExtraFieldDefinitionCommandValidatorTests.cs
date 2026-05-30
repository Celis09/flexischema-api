using ContactsAPI.Application.Admins.Commands.UpdateExtraFieldDefinition;
using ContactsAPI.Entities;
using ContactsAPI.Models;
using Xunit;

namespace ContactsAPI.Test.Validators.AdminValidators;

public class UpdateExtraFieldDefinitionCommandValidatorTests
{
    // ── ID rule ───────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_InvalidId_Fails(int id)
    {
        await using var ctx = ValidatorFixture.CreateContext(
            $"{nameof(Validate_InvalidId_Fails)}_{id}");
        var validator = new UpdateExtraFieldDefinitionCommandValidator(ctx);

        var result = await validator.ValidateAsync(new UpdateExtraFieldDefinitionCommand
        {
            ExtraFieldDefinitionId = id,
            FieldName = "Twitter",
            FieldType = ExtraFieldType.Text
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage.Contains("valid positive number"));
    }

    // ── Name uniqueness (excluding self) ──────────────────────────────────────

    [Fact]
    public async Task Validate_NameTakenByAnotherDefinition_Fails()
    {
        await using var ctx = ValidatorFixture.CreateContext(
            nameof(Validate_NameTakenByAnotherDefinition_Fails));

        ctx.ExtraFieldDefinitions.AddRange(
            new ExtraFieldDefinition { FieldName = "Twitter", FieldType = ExtraFieldType.Text },
            new ExtraFieldDefinition { FieldName = "LinkedIn", FieldType = ExtraFieldType.Url }
        );
        await ctx.SaveChangesAsync();

        var defs = ctx.ExtraFieldDefinitions.ToList();
        var linkedin = defs.First(d => d.FieldName == "LinkedIn");

        var validator = new UpdateExtraFieldDefinitionCommandValidator(ctx);
        var result = await validator.ValidateAsync(new UpdateExtraFieldDefinitionCommand
        {
            ExtraFieldDefinitionId = linkedin.ExtraFieldDefinitionId,
            FieldName = "Twitter",   // name already taken by another definition
            FieldType = ExtraFieldType.Text
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == "A field with this name already exists");
    }

    [Fact]
    public async Task Validate_SameNameAsOwnDefinition_Passes()
    {
        await using var ctx = ValidatorFixture.CreateContext(
            nameof(Validate_SameNameAsOwnDefinition_Passes));

        ctx.ExtraFieldDefinitions.Add(
            new ExtraFieldDefinition { FieldName = "Twitter", FieldType = ExtraFieldType.Text });
        await ctx.SaveChangesAsync();

        var def = ctx.ExtraFieldDefinitions.First();
        var validator = new UpdateExtraFieldDefinitionCommandValidator(ctx);

        var result = await validator.ValidateAsync(new UpdateExtraFieldDefinitionCommand
        {
            ExtraFieldDefinitionId = def.ExtraFieldDefinitionId,
            FieldName = "Twitter",  // same name — updating own record; should be allowed
            FieldType = ExtraFieldType.Url
        });

        Assert.DoesNotContain(result.Errors,
            e => e.ErrorMessage == "A field with this name already exists");
    }
}