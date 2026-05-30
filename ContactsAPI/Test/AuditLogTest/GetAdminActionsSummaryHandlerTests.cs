using ContactsAPI.Application.AuditLogs.Queries.GetAdminActionsSummary;
using ContactsAPI.Data;
using ContactsAPI.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ContactsAPI.Test.AuditLogTest;

public class GetAdminActionsSummaryHandlerTests
{
    private static ContactsDbContext Ctx(string name) =>
        new(new DbContextOptionsBuilder<ContactsDbContext>()
            .UseInMemoryDatabase(name)
            .Options);

    // ── Grouping and counting ─────────────────────────────────────────────────

    [Fact]
    public async Task Handle_GroupsByActionTypeAndCounts()
    {
        await using var ctx = Ctx(nameof(Handle_GroupsByActionTypeAndCounts));

        var from = DateTime.UtcNow.AddHours(-1);
        ctx.AuditLogs.AddRange(
            new AuditLog { ActionType = "CreateContact", EntityName = "Contact", EntityId = "1", Timestamp = DateTime.UtcNow },
            new AuditLog { ActionType = "CreateContact", EntityName = "Contact", EntityId = "2", Timestamp = DateTime.UtcNow },
            new AuditLog { ActionType = "DeleteContact", EntityName = "Contact", EntityId = "3", Timestamp = DateTime.UtcNow }
        );
        await ctx.SaveChangesAsync();

        var filter = new AdminActionSummaryFilter
        {
            FromDate = from,
            Page = 1,
            PageSize = 10,
            SortBy = "Count",
            SortOrder = "desc"
        };

        var result = await new GetAdminActionsSummaryHandler(ctx).Handle(
            new GetAdminActionsSummaryQuery(filter), CancellationToken.None);

        Assert.Equal(2, result.TotalCount);  // 2 distinct action types

        var createItem = result.Items.First(i => i.ActionType == "CreateContact");
        var deleteItem = result.Items.First(i => i.ActionType == "DeleteContact");
        Assert.Equal(2, createItem.Count);
        Assert.Equal(1, deleteItem.Count);

        // SortOrder = desc by Count → CreateContact (2) before DeleteContact (1)
        Assert.Equal("CreateContact", result.Items[0].ActionType);
    }

    // ── Role filter ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithRoleFilter_ReturnsOnlyMatchingRole()
    {
        await using var ctx = Ctx(nameof(Handle_WithRoleFilter_ReturnsOnlyMatchingRole));

        var from = DateTime.UtcNow.AddHours(-1);
        ctx.AuditLogs.AddRange(
            new AuditLog { ActionType = "CreateContact", UserRole = "Admin", EntityName = "Contact", EntityId = "1", Timestamp = DateTime.UtcNow },
            new AuditLog { ActionType = "UpdateContact", UserRole = "Editor", EntityName = "Contact", EntityId = "2", Timestamp = DateTime.UtcNow }
        );
        await ctx.SaveChangesAsync();

        var filter = new AdminActionSummaryFilter
        {
            FromDate = from,
            Role = "Admin",
            Page = 1,
            PageSize = 10
        };

        var result = await new GetAdminActionsSummaryHandler(ctx).Handle(
            new GetAdminActionsSummaryQuery(filter), CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal("CreateContact", result.Items[0].ActionType);
    }

    // ── Date range filter ─────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithFromAndToDate_ReturnsLogsInRange()
    {
        await using var ctx = Ctx(nameof(Handle_WithFromAndToDate_ReturnsLogsInRange));

        var from = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc);

        ctx.AuditLogs.AddRange(
            new AuditLog { ActionType = "Before", EntityName = "X", EntityId = "1", Timestamp = from.AddDays(-1) },
            new AuditLog { ActionType = "InRange", EntityName = "X", EntityId = "2", Timestamp = from.AddDays(5) },
            new AuditLog { ActionType = "After", EntityName = "X", EntityId = "3", Timestamp = to.AddDays(5) }
        );
        await ctx.SaveChangesAsync();

        var filter = new AdminActionSummaryFilter
        {
            FromDate = from,
            ToDate = to,
            Page = 1,
            PageSize = 10
        };

        var result = await new GetAdminActionsSummaryHandler(ctx).Handle(
            new GetAdminActionsSummaryQuery(filter), CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal("InRange", result.Items[0].ActionType);
    }

    // ── Empty result ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithNoLogs_ReturnsEmptyPagedResult()
    {
        await using var ctx = Ctx(nameof(Handle_WithNoLogs_ReturnsEmptyPagedResult));

        var filter = new AdminActionSummaryFilter
        {
            FromDate = DateTime.UtcNow.AddHours(-1),
            Page = 1,
            PageSize = 10
        };

        var result = await new GetAdminActionsSummaryHandler(ctx).Handle(
            new GetAdminActionsSummaryQuery(filter), CancellationToken.None);

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }
}