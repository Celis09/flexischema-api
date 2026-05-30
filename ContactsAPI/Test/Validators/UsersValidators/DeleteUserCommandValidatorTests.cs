using ContactsAPI.Application.Users.Commands.DeleteUser;
using Xunit;

namespace ContactsAPI.Test.Validators.UsersValidators;

public class DeleteUserCommandValidatorTests
{
    private readonly DeleteUserCommandValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validate_InvalidUserId_Fails(int id)
    {
        var result = _validator.Validate(new DeleteUserCommand { UserId = id });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ValidUserId_Passes()
    {
        var result = _validator.Validate(new DeleteUserCommand { UserId = 1 });

        Assert.True(result.IsValid);
    }
}