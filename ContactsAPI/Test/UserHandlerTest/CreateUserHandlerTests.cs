using ContactsAPI.Application.Users.Commands.CreateUser;
using ContactsAPI.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ContactsAPI.Test.UserHandlerTest;

public class CreateUserHandlerTests
{
    private ContactsDbContext GetDbContext() =>
        new(new DbContextOptionsBuilder<ContactsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task CreateUserHandler_ShouldCreateUser()
    {
        var context = GetDbContext();
        var handler = new CreateUserHandler(context);

        var command = new CreateUserCommand
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "hashedpwd",
            Role = "Editor"
        };

        var userId = await handler.Handle(command, default);

        var user = await context.Users.FindAsync(userId);
        Assert.NotNull(user);
        Assert.Equal("testuser", user.Username);
        Assert.Equal("Editor", user.Role);
    }
}