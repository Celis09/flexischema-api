using ContactsAPI.Application.Admins.Commands.UpdateAdminConfig;
using Xunit;

namespace ContactsAPI.Test.Validators.AdminValidators;

public class UpdateAdminConfigCommandValidatorTests
{
    private readonly UpdateAdminConfigCommandValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_InvalidId_Fails(int id)
    {
        var result = _validator.Validate(new UpdateAdminConfigCommand
        {
            Id = id,
            Key = "SomeKey",
            Value = "5"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Config ID must be valid");
    }

    [Fact]
    public void Validate_EmptyKey_Fails()
    {
        var result = _validator.Validate(new UpdateAdminConfigCommand
        {
            Id = 1,
            Key = "",
            Value = "5"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Config key is required");
    }

    [Fact]
    public void Validate_EmptyValue_Fails()
    {
        var result = _validator.Validate(new UpdateAdminConfigCommand
        {
            Id = 1,
            Key = "SomeKey",
            Value = ""
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Config value is required");
    }

    // ── Special MaxExtraFieldsPerContact rule ─────────────────────────────────

    [Theory]
    [InlineData("16")]
    [InlineData("100")]
    [InlineData("abc")]   // non-numeric also fails the Must() rule
    public void Validate_MaxExtraFieldsExceeds15OrNonNumeric_Fails(string value)
    {
        var result = _validator.Validate(new UpdateAdminConfigCommand
        {
            Id = 1,
            Key = "MaxExtraFieldsPerContact",
            Value = value
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == "MaxExtraFieldsPerContact cannot exceed 15");
    }

    [Theory]
    [InlineData("1")]
    [InlineData("15")]
    public void Validate_MaxExtraFieldsWithinLimit_Passes(string value)
    {
        var result = _validator.Validate(new UpdateAdminConfigCommand
        {
            Id = 1,
            Key = "MaxExtraFieldsPerContact",
            Value = value
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_OtherKeyWithHighValue_Passes()
    {
        // The ≤15 cap only applies when Key == "MaxExtraFieldsPerContact"
        var result = _validator.Validate(new UpdateAdminConfigCommand
        {
            Id = 1,
            Key = "SomeOtherConfig",
            Value = "999"
        });

        Assert.True(result.IsValid);
    }
}