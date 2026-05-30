using ContactsAPI.Application.Admins.Queries.GetExtraFieldDefinitions;
using Xunit;

namespace ContactsAPI.Test.Validators.AdminValidators;

public class GetExtraFieldDefinitionsQueryValidatorTests
{
    private readonly GetExtraFieldDefinitionsQueryValidator _validator = new();

    [Theory]
    [InlineData("Viewer")]
    [InlineData("SuperAdmin")]
    [InlineData("editor")]    // case-sensitive — "editor" != "Editor"
    public void Validate_InvalidRoleFilter_Fails(string role)
    {
        var result = _validator.Validate(
            new GetExtraFieldDefinitionsQuery { RoleFilter = role });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == "Role filter must be Admin or Editor");
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Editor")]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_ValidOrNullRoleFilter_Passes(string? role)
    {
        var result = _validator.Validate(
            new GetExtraFieldDefinitionsQuery { RoleFilter = role });

        Assert.True(result.IsValid);
    }
}