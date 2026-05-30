using ContactsAPI.Application.Abstractions;
using ContactsAPI.Application.Exceptions;
using ContactsAPI.Application.Users.Commands.ChangeUserStatus;
using ContactsAPI.Models;
using ContactsAPI.Test.AdminTestHandler.Helpers;
using Xunit;

namespace ContactsAPI.Test.UserHandlerTest;

public class ChangeUserStatusHandlerTests
{
    // Minimal IUserContext stub — returns whatever userId is passed in.
    private sealed class FakeUserContext(string userId) : IUserContext
    {
        public string UserId => userId;
        public string Role => "Admin";
    }

    // -------------------------------------------------------------------------
    // Happy path
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(UserStatus.Inactive)]
    [InlineData(UserStatus.Suspended)]
    public async Task Handle_WithExistingUser_UpdatesStatusCorrectly(UserStatus newStatus)
    {
        var dbName = $"{nameof(Handle_WithExistingUser_UpdatesStatusCorrectly)}_{newStatus}";
        await using var context = AdminDbFactory.Create(dbName);

        // UserId = 10 — safely outside the protected set {1, 2}.
        var user = new User
        {
            UserId = 10,
            Username = "StatusUser",
            Email = "status@example.com",
            Status = UserStatus.Active
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Current user is someone else (id 99), so Guard 2 does not trigger.
        var handler = new ChangeUserStatusHandler(context, new FakeUserContext("99"));
        var command = new ChangeUserStatusCommand { UserId = user.UserId, Status = newStatus };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result);
        var updated = await context.Users.FindAsync(user.UserId);
        Assert.Equal(newStatus, updated!.Status);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ReturnsFalse()
    {
        await using var context = AdminDbFactory.Create(
            nameof(Handle_WithNonExistentUser_ReturnsFalse));

        var handler = new ChangeUserStatusHandler(context, new FakeUserContext("99"));
        var command = new ChangeUserStatusCommand
        {
            UserId = 9999,
            Status = UserStatus.Suspended
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result);
    }

    // -------------------------------------------------------------------------
    // Guard 1 — seeded demo accounts are permanently protected
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Handle_ProtectedUser_ThrowsUnauthorizedException(int protectedId)
    {
        await using var context = AdminDbFactory.Create(
            $"{nameof(Handle_ProtectedUser_ThrowsUnauthorizedException)}_{protectedId}");

        var handler = new ChangeUserStatusHandler(context, new FakeUserContext("99"));
        var command = new ChangeUserStatusCommand
        {
            UserId = protectedId,
            Status = UserStatus.Inactive
        };

        await Assert.ThrowsAsync<UnauthorizedAccessAppException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    // -------------------------------------------------------------------------
    // Guard 2 — a user cannot demote their own account
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(UserStatus.Inactive)]
    [InlineData(UserStatus.Suspended)]
    public async Task Handle_SelfDemotion_ThrowsUnauthorizedException(UserStatus newStatus)
    {
        var dbName = $"{nameof(Handle_SelfDemotion_ThrowsUnauthorizedException)}_{newStatus}";
        await using var context = AdminDbFactory.Create(dbName);

        var user = new User
        {
            UserId = 10,
            Username = "SelfUser",
            Email = "self@example.com",
            Status = UserStatus.Active
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Current user IS the target user.
        var handler = new ChangeUserStatusHandler(context, new FakeUserContext("10"));
        var command = new ChangeUserStatusCommand { UserId = 10, Status = newStatus };

        await Assert.ThrowsAsync<UnauthorizedAccessAppException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SelfSetActive_Succeeds()
    {
        await using var context = AdminDbFactory.Create(
            nameof(Handle_SelfSetActive_Succeeds));

        var user = new User
        {
            UserId = 10,
            Username = "SelfUser",
            Email = "self@example.com",
            Status = UserStatus.Inactive
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // A user reactivating themselves is allowed.
        var handler = new ChangeUserStatusHandler(context, new FakeUserContext("10"));
        var command = new ChangeUserStatusCommand { UserId = 10, Status = UserStatus.Active };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result);
        var updated = await context.Users.FindAsync(user.UserId);
        Assert.Equal(UserStatus.Active, updated!.Status);
    }
}