using ContactsAPI.Application.Users.Queries.GetUserById;
using ContactsAPI.Data;
using ContactsAPI.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ContactsAPI.Test.UserHandlerTest;

public class GetUserByIdHandlerTests
{
    private ContactsDbContext GetDbContext() =>
        new(new DbContextOptionsBuilder<ContactsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_ShouldReturnUser_WhenExists()
    {
        var context = GetDbContext();
        var user = new User { Username = "john", Email = "john@example.com", Role = "Admin" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var handler = new GetUserByIdHandler(context);
        var result = await handler.Handle(
            new GetUserByIdQuery { UserId = user.UserId }, default);

        Assert.NotNull(result);
        Assert.Equal("john", result.Username);
        Assert.Equal("Admin", result.Role);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenUserNotFound()
    {
        var context = GetDbContext();
        var handler = new GetUserByIdHandler(context);

        var result = await handler.Handle(
            new GetUserByIdQuery { UserId = 999 }, default);

        Assert.Null(result);
    }
}