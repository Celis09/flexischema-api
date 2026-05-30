using ContactsAPI.Application.Abstractions;
using ContactsAPI.Application.Exceptions;
using ContactsAPI.Application.Users.Commands.UpdateUser;
using ContactsAPI.Data;
using ContactsAPI.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ContactsAPI.Test.UserHandlerTest;

public class UpdateUserHandlerTests
{
    private sealed class FakeUserContext(string userId) : IUserContext
    {
        public string UserId => userId;
        public string Role => "Admin";
    }

    private static ContactsDbContext GetDbContext() =>
        new(new DbContextOptionsBuilder<ContactsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    // -------------------------------------------------------------------------
    // Happy path
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldUpdateUser_WhenExists()
    {
        var context = GetDbContext();

        // UserId = 10 — safely outside the protected set {1, 2}.
        var user = new User { UserId = 10, Username = "old", Email = "old@example.com", Role = "Editor" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Current user is someone else (id 99), so Guard 2 does not trigger.
        var handler = new UpdateUserHandler(context, new FakeUserContext("99"));
        var command = new UpdateUserCommand
        {
            UserId = 10,
            Username = "new",
            Email = "new@example.com",
            Role = "Editor"
        };

        var result = await handler.Handle(command, default);

        Assert.True(result);
        var updated = await context.Users.FindAsync(user.UserId);
        Assert.Equal("new", updated!.Username);
        Assert.Equal("Editor", updated.Role);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenUserNotFound()
    {
        var context = GetDbContext();
        var handler = new UpdateUserHandler(context, new FakeUserContext("99"));

        var act = async () => await handler.Handle(
            new UpdateUserCommand { UserId = 999, Username = "ghost" }, default);

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    // -------------------------------------------------------------------------
    // Guard 1 — seeded demo accounts cannot be edited
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Handle_ProtectedUser_ThrowsUnauthorizedException(int protectedId)
    {
        var context = GetDbContext();
        var handler = new UpdateUserHandler(context, new FakeUserContext("99"));
        var command = new UpdateUserCommand
        {
            UserId = protectedId,
            Username = "hacked",
            Email = "hacked@example.com",
            Role = "Editor"
        };

        await Assert.ThrowsAsync<UnauthorizedAccessAppException>(
            () => handler.Handle(command, default));
    }

    // -------------------------------------------------------------------------
    // Guard 2 — a user cannot demote their own role
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Handle_SelfRoleDemotion_ThrowsUnauthorizedException()
    {
        var context = GetDbContext();
        var user = new User { UserId = 10, Username = "admin", Email = "admin@example.com", Role = "Admin" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Current user IS the target user.
        var handler = new UpdateUserHandler(context, new FakeUserContext("10"));
        var command = new UpdateUserCommand
        {
            UserId = 10,
            Username = "admin",
            Email = "admin@example.com",
            Role = "Editor"  // demotion attempt
        };

        await Assert.ThrowsAsync<UnauthorizedAccessAppException>(
            () => handler.Handle(command, default));
    }

    [Fact]
    public async Task Handle_SelfUpdateKeepingAdminRole_Succeeds()
    {
        var context = GetDbContext();
        var user = new User { UserId = 10, Username = "admin", Email = "admin@example.com", Role = "Admin" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Updating own username/email while keeping the Admin role is allowed.
        var handler = new UpdateUserHandler(context, new FakeUserContext("10"));
        var command = new UpdateUserCommand
        {
            UserId = 10,
            Username = "admin-updated",
            Email = "admin-updated@example.com",
            Role = "Admin"
        };

        var result = await handler.Handle(command, default);

        Assert.True(result);
        var updated = await context.Users.FindAsync(user.UserId);
        Assert.Equal("admin-updated", updated!.Username);
        Assert.Equal("Admin", updated.Role);
    }
}