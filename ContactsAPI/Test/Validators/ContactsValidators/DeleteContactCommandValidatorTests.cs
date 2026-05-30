using ContactsAPI.Application.Contacts.Commands.DeleteContact;
using Xunit;

namespace ContactsAPI.Test.Validators.ContactsValidators;

public class DeleteContactCommandValidatorTests
{
    private readonly DeleteContactCommandValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_InvalidId_Fails(int id)
    {
        var result = _validator.Validate(new DeleteContactCommand { Id = id });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage.Contains("valid positive number"));
    }

    [Fact]
    public void Validate_ValidId_Passes()
    {
        var result = _validator.Validate(new DeleteContactCommand { Id = 1 });

        Assert.True(result.IsValid);
    }
}