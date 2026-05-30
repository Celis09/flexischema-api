using ContactsAPI.Application.AuditLogs.Queries.GetAllAuditLogs;
using ContactsAPI.Data;
using ContactsAPI.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ContactsAPI.Test.AuditLogTest;

public class GetAllAuditLogsHandlerTests
{
    private static ContactsDbContext Ctx(string name) =>
        new(new DbContextOptionsBuilder<ContactsDbContext>()
            .UseInMemoryDatabase(name)
            .Options);

    // ── Basic paging ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ReturnsPagedResults()
    {
        await using var ctx = Ctx(nameof(Handle_ReturnsPagedResults));

        ctx.AuditLogs.AddRange(Enumerable.Range(1, 15).Select(i => new AuditLog
        {
            ActionType = "CreateContact",
            EntityName = "Contact",
            EntityId = i.ToString()
        }));
        await ctx.SaveChangesAsync();

        var handler = new GetAllAuditLogsHandler(ctx);
        var result = await handler.Handle(
            new GetAllAuditLogsQuery { Page = 2, PageSize = 5 }, CancellationToken.None);

        Assert.Equal(15, result.TotalCount);
        Assert.Equal(5, result.Items.Count);
        Assert.Equal(2, result.Page);
    }

    // ── ActionType filter ─────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithActionTypeFilter_ReturnsOnlyMatchingLogs()
    {
        await using var ctx = Ctx(nameof(Handle_WithActionTypeFilter_ReturnsOnlyMatchingLogs));

        ctx.AuditLogs.AddRange(
            new AuditLog { ActionType = "CreateContact", EntityName = "Contact", EntityId = "1" },
            new AuditLog { ActionType = "DeleteContact", EntityName = "Contact", EntityId = "2" },
            new AuditLog { ActionType = "CreateContact", EntityName = "Contact", EntityId = "3" }
        );
        await ctx.SaveChangesAsync();

        var result = await new GetAllAuditLogsHandler(ctx).Handle(
            new GetAllAuditLogsQuery { ActionType = "DeleteContact", Page = 1, PageSize = 10 },
            CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.All(result.Items, item => Assert.Equal("DeleteContact", item.ActionType));
    }

    // ── Success filter ────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithSuccessFilter_ReturnsOnlyFailedLogs()
    {
        await using var ctx = Ctx(nameof(Handle_WithSuccessFilter_ReturnsOnlyFailedLogs));

        ctx.AuditLogs.AddRange(
            new AuditLog { ActionType = "CreateContact", EntityName = "Contact", EntityId = "1", Success = true },
            new AuditLog { ActionType = "CreateContact", EntityName = "Contact", EntityId = "2", Success = false },
            new AuditLog { ActionType = "CreateContact", EntityName = "Contact", EntityId = "3", Success = false }
        );
        await ctx.SaveChangesAsync();

        var result = await new GetAllAuditLogsHandler(ctx).Handle(
            new GetAllAuditLogsQuery { Success = false, Page = 1, PageSize = 10 },
            CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, item => Assert.False(item.Success));
    }

    // ── Empty result ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithNoLogs_ReturnsEmptyPagedResult()
    {
        await using var ctx = Ctx(nameof(Handle_WithNoLogs_ReturnsEmptyPagedResult));

        var result = await new GetAllAuditLogsHandler(ctx).Handle(
            new GetAllAuditLogsQuery { Page = 1, PageSize = 10 }, CancellationToken.None);

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }
}