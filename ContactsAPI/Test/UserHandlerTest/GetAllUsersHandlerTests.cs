using ContactsAPI.Application.Users.Queries.GetAllUsers;
using ContactsAPI.Data;
using ContactsAPI.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ContactsAPI.Test.UserHandlerTest;

public class GetAllUsersHandlerTests
{
    private static ContactsDbContext Ctx(string name) =>
        new(new DbContextOptionsBuilder<ContactsDbContext>()
            .UseInMemoryDatabase(name)
            .Options);

    // ── Basic return ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ShouldReturnAllUsers()
    {
        await using var ctx = Ctx(nameof(Handle_ShouldReturnAllUsers));
        ctx.Users.AddRange(
            new User { Username = "u1", Email = "u1@example.com", Role = "Viewer" },
            new User { Username = "u2", Email = "u2@example.com", Role = "Admin" }
        );
        await ctx.SaveChangesAsync();

        var result = await new GetAllUsersHandler(ctx).Handle(
            new GetAllUsersQuery { Page = 1, PageSize = 10 }, default);

        Assert.Equal(2, result.TotalCount);
        Assert.Contains(result.Items, u => u.Role == "Viewer");
        Assert.Contains(result.Items, u => u.Role == "Admin");
    }

    // ── Search / filter ───────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ShouldFilterByRoleSearch()
    {
        await using var ctx = Ctx(nameof(Handle_ShouldFilterByRoleSearch));
        ctx.Users.AddRange(
            new User { Username = "u1", Email = "u1@example.com", Role = "Viewer" },
            new User { Username = "u2", Email = "u2@example.com", Role = "Admin" }
        );
        await ctx.SaveChangesAsync();

        var result = await new GetAllUsersHandler(ctx).Handle(
            new GetAllUsersQuery { Search = "Admin", Page = 1, PageSize = 10 }, default);

        Assert.Single(result.Items);
        Assert.Equal("Admin", result.Items[0].Role);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ReturnsOnlyMatchingUsers()
    {
        await using var ctx = Ctx(nameof(Handle_WithStatusFilter_ReturnsOnlyMatchingUsers));
        ctx.Users.AddRange(
            new User { Username = "active", Email = "a@example.com", Status = UserStatus.Active },
            new User { Username = "inactive", Email = "b@example.com", Status = UserStatus.Inactive },
            new User { Username = "suspended", Email = "c@example.com", Status = UserStatus.Suspended }
        );
        await ctx.SaveChangesAsync();

        var result = await new GetAllUsersHandler(ctx).Handle(
            new GetAllUsersQuery { Status = UserStatus.Inactive, Page = 1, PageSize = 10 }, default);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal("inactive", result.Items[0].Username);
    }

    // ── Date range filter ─────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithFromDateFilter_ExcludesUsersCreatedBefore()
    {
        await using var ctx = Ctx(nameof(Handle_WithFromDateFilter_ExcludesUsersCreatedBefore));

        var cutoff = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        ctx.Users.AddRange(
            new User { Username = "old", Email = "old@example.com", CreatedDate = cutoff.AddDays(-1) },
            new User { Username = "new", Email = "new@example.com", CreatedDate = cutoff.AddDays(1) }
        );
        await ctx.SaveChangesAsync();

        var result = await new GetAllUsersHandler(ctx).Handle(
            new GetAllUsersQuery { FromDate = cutoff, Page = 1, PageSize = 10 }, default);

        Assert.Equal(1, result.TotalCount);
        Assert.DoesNotContain(result.Items, u => u.Username == "old");
    }

    [Fact]
    public async Task Handle_WithToDateFilter_ExcludesUsersCreatedAfter()
    {
        await using var ctx = Ctx(nameof(Handle_WithToDateFilter_ExcludesUsersCreatedAfter));

        var cutoff = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        ctx.Users.AddRange(
            new User { Username = "early", Email = "early@example.com", CreatedDate = cutoff.AddDays(-1) },
            new User { Username = "future", Email = "future@example.com", CreatedDate = cutoff.AddDays(5) }
        );
        await ctx.SaveChangesAsync();

        var result = await new GetAllUsersHandler(ctx).Handle(
            new GetAllUsersQuery { ToDate = cutoff, Page = 1, PageSize = 10 }, default);

        Assert.Equal(1, result.TotalCount);
        Assert.DoesNotContain(result.Items, u => u.Username == "future");
    }

    // ── Pagination ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_Pagination_ReturnsCorrectPage()
    {
        await using var ctx = Ctx(nameof(Handle_Pagination_ReturnsCorrectPage));
        ctx.Users.AddRange(Enumerable.Range(1, 12).Select(i =>
            new User { Username = $"user{i:D2}", Email = $"u{i}@example.com" }));
        await ctx.SaveChangesAsync();

        var result = await new GetAllUsersHandler(ctx).Handle(
            new GetAllUsersQuery { Page = 2, PageSize = 5 }, default);

        Assert.Equal(12, result.TotalCount);
        Assert.Equal(5, result.Items.Count);
        Assert.Equal(2, result.Page);
        Assert.True(result.HasPrev);
        Assert.True(result.HasNext);
    }

    [Fact]
    public async Task Handle_WithNoUsers_ReturnsEmptyPagedResult()
    {
        await using var ctx = Ctx(nameof(Handle_WithNoUsers_ReturnsEmptyPagedResult));

        var result = await new GetAllUsersHandler(ctx).Handle(
            new GetAllUsersQuery { Page = 1, PageSize = 10 }, default);

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }
}