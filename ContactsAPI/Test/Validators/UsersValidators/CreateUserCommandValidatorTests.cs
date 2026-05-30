using ContactsAPI.Application.Users.Commands.CreateUser;
using Xunit;

namespace ContactsAPI.Test.Validators.UsersValidators;

public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator = new();

    // ── Username ──────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_EmptyUsername_Fails()
    {
        var result = _validator.Validate(new CreateUserCommand
        {
            Username = "",
            Email = "user@example.com",
            Password = "Secret123!",
            Role = "Editor"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Username is required");
    }

    [Fact]
    public void Validate_UsernameTooLong_Fails()
    {
        var result = _validator.Validate(new CreateUserCommand
        {
            Username = new string('x', 51),
            Email = "user@example.com",
            Password = "Secret123!",
            Role = "Editor"
        });

        Assert.False(result.IsValid);
    }

    // ── Email ─────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing@")]          // ← no domain at all — always fails
    public void Validate_InvalidEmail_Fails(string email)
    {
        var result = _validator.Validate(new CreateUserCommand
        {
            Username = "alice",
            Email = email,
            Password = "Secret123!",
            Role = "Editor"
        });

        Assert.False(result.IsValid);
    }

    // ── Password ──────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_EmptyPassword_Fails()
    {
        var result = _validator.Validate(new CreateUserCommand
        {
            Username = "alice",
            Email = "alice@example.com",
            Password = "",
            Role = "Editor"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Password is required");
    }

    // ── Role ──────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Viewer")]
    [InlineData("SuperAdmin")]
    [InlineData("")]
    public void Validate_InvalidRole_Fails(string role)
    {
        var result = _validator.Validate(new CreateUserCommand
        {
            Username = "alice",
            Email = "alice@example.com",
            Password = "Secret123!",
            Role = role
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Role must be Admin or Editor");
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Editor")]
    public void Validate_ValidRole_Passes(string role)
    {
        var result = _validator.Validate(new CreateUserCommand
        {
            Username = "alice",
            Email = "alice@example.com",
            Password = "Secret123!",
            Role = role
        });

        Assert.True(result.IsValid);
    }
}