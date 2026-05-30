using ContactsAPI.Application.Contacts.Dtos;
using ContactsAPI.Application.Contacts.Validators;
using ContactsAPI.Data;
using ContactsAPI.Entities;
using ContactsAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace ContactsAPI.Test.Validators.ContactsValidators;

public class ContactExtraFieldValidatorTests
{
    private static (ContactExtraFieldValidator validator, ContactsDbContext ctx)
        Build(string dbName)
    {
        var ctx = new ContactsDbContext(
            new DbContextOptionsBuilder<ContactsDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options);

        var cache = new MemoryCache(new MemoryCacheOptions());

        return (new ContactExtraFieldValidator(ctx, cache), ctx);
    }

    // ── Definition ID ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_InvalidDefinitionId_Fails(int id)
    {
        var (validator, _) = Build($"{nameof(Validate_InvalidDefinitionId_Fails)}_{id}");

        var result = await validator.ValidateAsync(
            new ContactExtraFieldRequest { ExtraFieldDefinitionId = id, FieldValue = "test" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == "Extra field definition ID must be valid");
    }

    // ── Unknown definition ────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_NonExistentDefinitionId_Fails()
    {
        var (validator, _) = Build(nameof(Validate_NonExistentDefinitionId_Fails));

        var result = await validator.ValidateAsync(
            new ContactExtraFieldRequest { ExtraFieldDefinitionId = 999, FieldValue = "test" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage.Contains("not found"));
    }

    // ── Value too long ────────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_ValueExceeds200Chars_Fails()
    {
        var (validator, ctx) = Build(nameof(Validate_ValueExceeds200Chars_Fails));

        ctx.ExtraFieldDefinitions.Add(new ExtraFieldDefinition
        {
            FieldName = "Notes",
            FieldType = ExtraFieldType.Text,
            IsActive = true
        });
        await ctx.SaveChangesAsync();

        var def = ctx.ExtraFieldDefinitions.First();

        var result = await validator.ValidateAsync(new ContactExtraFieldRequest
        {
            ExtraFieldDefinitionId = def.ExtraFieldDefinitionId,
            FieldValue = new string('x', 201)
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == "Extra field value must not exceed 200 characters");
    }

    // ── Required field left blank ─────────────────────────────────────────────

    [Fact]
    public async Task Validate_RequiredFieldLeftBlank_Fails()
    {
        var (validator, ctx) = Build(nameof(Validate_RequiredFieldLeftBlank_Fails));

        ctx.ExtraFieldDefinitions.Add(new ExtraFieldDefinition
        {
            FieldName = "Phone",
            FieldType = ExtraFieldType.Phone,
            IsActive = true,
            IsRequired = true
        });
        await ctx.SaveChangesAsync();

        var def = ctx.ExtraFieldDefinitions.First();

        var result = await validator.ValidateAsync(new ContactExtraFieldRequest
        {
            ExtraFieldDefinitionId = def.ExtraFieldDefinitionId,
            FieldValue = ""
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Phone") &&
                                            e.ErrorMessage.Contains("required"));
    }

    // ── Type-format: Email ────────────────────────────────────────────────────

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing@domain")]
    public async Task Validate_InvalidEmailFormat_Fails(string value)
    {
        var (validator, ctx) = Build($"{nameof(Validate_InvalidEmailFormat_Fails)}_{value}");

        ctx.ExtraFieldDefinitions.Add(new ExtraFieldDefinition
        {
            FieldName = "WorkEmail",
            FieldType = ExtraFieldType.Email,
            IsActive = true
        });
        await ctx.SaveChangesAsync();

        var def = ctx.ExtraFieldDefinitions.First();

        var result = await validator.ValidateAsync(new ContactExtraFieldRequest
        {
            ExtraFieldDefinitionId = def.ExtraFieldDefinitionId,
            FieldValue = value
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage.Contains("valid email address"));
    }

    // ── Type-format: Phone ────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_InvalidPhoneFormat_Fails()
    {
        var (validator, ctx) = Build(nameof(Validate_InvalidPhoneFormat_Fails));

        ctx.ExtraFieldDefinitions.Add(new ExtraFieldDefinition
        {
            FieldName = "Mobile",
            FieldType = ExtraFieldType.Phone,
            IsActive = true
        });
        await ctx.SaveChangesAsync();

        var def = ctx.ExtraFieldDefinitions.First();

        var result = await validator.ValidateAsync(new ContactExtraFieldRequest
        {
            ExtraFieldDefinitionId = def.ExtraFieldDefinitionId,
            FieldValue = "not-a-phone"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage.Contains("valid phone number"));
    }

    // ── Type-format: URL ──────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_InvalidUrl_Fails()
    {
        var (validator, ctx) = Build(nameof(Validate_InvalidUrl_Fails));

        ctx.ExtraFieldDefinitions.Add(new ExtraFieldDefinition
        {
            FieldName = "Website",
            FieldType = ExtraFieldType.Url,
            IsActive = true
        });
        await ctx.SaveChangesAsync();

        var def = ctx.ExtraFieldDefinitions.First();

        var result = await validator.ValidateAsync(new ContactExtraFieldRequest
        {
            ExtraFieldDefinitionId = def.ExtraFieldDefinitionId,
            FieldValue = "not a url"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage.Contains("valid URL"));
    }

    // ── Type-format: Number ───────────────────────────────────────────────────

    [Fact]
    public async Task Validate_NonNumericValue_ForNumberField_Fails()
    {
        var (validator, ctx) = Build(nameof(Validate_NonNumericValue_ForNumberField_Fails));

        ctx.ExtraFieldDefinitions.Add(new ExtraFieldDefinition
        {
            FieldName = "Age",
            FieldType = ExtraFieldType.Number,
            IsActive = true
        });
        await ctx.SaveChangesAsync();

        var def = ctx.ExtraFieldDefinitions.First();

        var result = await validator.ValidateAsync(new ContactExtraFieldRequest
        {
            ExtraFieldDefinitionId = def.ExtraFieldDefinitionId,
            FieldValue = "twenty-five"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage.Contains("valid number"));
    }

    // ── Type-format: Option ───────────────────────────────────────────────────

    [Fact]
    public async Task Validate_ValueNotInOptions_Fails()
    {
        var (validator, ctx) = Build(nameof(Validate_ValueNotInOptions_Fails));

        var def = new ExtraFieldDefinition
        {
            FieldName = "Department",
            FieldType = ExtraFieldType.Option,
            IsActive = true
        };
        ctx.ExtraFieldDefinitions.Add(def);
        await ctx.SaveChangesAsync();

        ctx.ExtraFieldOptions.AddRange(
            new ExtraFieldOption { ExtraFieldDefinitionId = def.ExtraFieldDefinitionId, OptionValue = "HR" },
            new ExtraFieldOption { ExtraFieldDefinitionId = def.ExtraFieldDefinitionId, OptionValue = "Engineering" }
        );
        await ctx.SaveChangesAsync();

        var result = await validator.ValidateAsync(new ContactExtraFieldRequest
        {
            ExtraFieldDefinitionId = def.ExtraFieldDefinitionId,
            FieldValue = "Finance"   // not in the list
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage.Contains("must be one of"));
    }

    [Fact]
    public async Task Validate_ValueInOptions_Passes()
    {
        var (validator, ctx) = Build(nameof(Validate_ValueInOptions_Passes));

        var def = new ExtraFieldDefinition
        {
            FieldName = "Department",
            FieldType = ExtraFieldType.Option,
            IsActive = true
        };
        ctx.ExtraFieldDefinitions.Add(def);
        await ctx.SaveChangesAsync();

        ctx.ExtraFieldOptions.Add(new ExtraFieldOption
        {
            ExtraFieldDefinitionId = def.ExtraFieldDefinitionId,
            OptionValue = "Engineering"
        });
        await ctx.SaveChangesAsync();

        var result = await validator.ValidateAsync(new ContactExtraFieldRequest
        {
            ExtraFieldDefinitionId = def.ExtraFieldDefinitionId,
            FieldValue = "Engineering"
        });

        Assert.True(result.IsValid);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_ValidTextField_Passes()
    {
        var (validator, ctx) = Build(nameof(Validate_ValidTextField_Passes));

        ctx.ExtraFieldDefinitions.Add(new ExtraFieldDefinition
        {
            FieldName = "Notes",
            FieldType = ExtraFieldType.Text,
            IsActive = true
        });
        await ctx.SaveChangesAsync();

        var def = ctx.ExtraFieldDefinitions.First();

        var result = await validator.ValidateAsync(new ContactExtraFieldRequest
        {
            ExtraFieldDefinitionId = def.ExtraFieldDefinitionId,
            FieldValue = "Some free-form text"
        });

        Assert.True(result.IsValid);
    }
}