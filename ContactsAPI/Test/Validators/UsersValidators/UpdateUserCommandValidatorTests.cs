using ContactsAPI.Application.Users.Commands.UpdateUser;
using Xunit;

namespace ContactsAPI.Test.Validators.UsersValidators;

public class UpdateUserCommandValidatorTests
{
    private readonly UpdateUserCommandValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Validate_InvalidUserId_Fails(int id)
    {
        var result = _validator.Validate(new UpdateUserCommand
        {
            UserId = id,
            Username = "alice",
            Email = "alice@example.com",
            Role = "Editor"
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyUsername_Fails()
    {
        var result = _validator.Validate(new UpdateUserCommand
        {
            UserId = 1,
            Username = "",
            Email = "alice@example.com",
            Role = "Editor"
        });

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Viewer")]
    [InlineData("")]
    public void Validate_InvalidRole_Fails(string role)
    {
        var result = _validator.Validate(new UpdateUserCommand
        {
            UserId = 1,
            Username = "alice",
            Email = "alice@example.com",
            Role = role
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        var result = _validator.Validate(new UpdateUserCommand
        {
            UserId = 1,
            Username = "alice",
            Email = "alice@example.com",
            Role = "Admin"
        });

        Assert.True(result.IsValid);
    }
}