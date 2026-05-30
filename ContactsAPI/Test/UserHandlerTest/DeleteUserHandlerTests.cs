using ContactsAPI.Application.Users.Commands.DeleteUser;
using ContactsAPI.Data;
using ContactsAPI.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ContactsAPI.Test.UserHandlerTest;

public class DeleteUserHandlerTests
{
    private ContactsDbContext GetDbContext() =>
        new(new DbContextOptionsBuilder<ContactsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_ShouldDeleteUser_WhenExists()
    {
        var context = GetDbContext();
        var user = new User { Username = "deleteMe", Email = "del@example.com" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var handler = new DeleteUserHandler(context);
        var result = await handler.Handle(
            new DeleteUserCommand { UserId = user.UserId }, default);

        Assert.True(result);
        Assert.Null(await context.Users.FindAsync(user.UserId));
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenUserNotFound()
    {
        var context = GetDbContext();
        var handler = new DeleteUserHandler(context);

        var result = await handler.Handle(
            new DeleteUserCommand { UserId = 999 }, default);

        Assert.False(result);
    }
}