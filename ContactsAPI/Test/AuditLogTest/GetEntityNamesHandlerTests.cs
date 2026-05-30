using ContactsAPI.Application.AuditLogs.Queries.GetAllAuditLogs;
using ContactsAPI.Data;
using ContactsAPI.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ContactsAPI.Test.AuditLogTest;

public class GetEntityNamesHandlerTests
{
    private static readonly string[] ExpectedEntityNames = ["Contact", "User"];

    private static ContactsDbContext Ctx(string name) =>
        new(new DbContextOptionsBuilder<ContactsDbContext>()
            .UseInMemoryDatabase(name)
            .Options);

    [Fact]
    public async Task Handle_ReturnsDistinctEntityNamesSorted()
    {
        await using var ctx = Ctx(nameof(Handle_ReturnsDistinctEntityNamesSorted));
        ctx.AuditLogs.AddRange(
            new AuditLog { ActionType = "Create", EntityName = "User", EntityId = "1" },
            new AuditLog { ActionType = "Delete", EntityName = "Contact", EntityId = "2" },
            new AuditLog { ActionType = "Update", EntityName = "User", EntityId = "3" }  // duplicate
        );
        await ctx.SaveChangesAsync();

        var result = await new GetEntityNamesHandler(ctx).Handle(
            new GetEntityNamesQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(ExpectedEntityNames, result.ToArray()); // sorted asc
    }

    [Fact]
    public async Task Handle_WithNoLogs_ReturnsEmptyList()
    {
        await using var ctx = Ctx(nameof(Handle_WithNoLogs_ReturnsEmptyList));

        var result = await new GetEntityNamesHandler(ctx).Handle(
            new GetEntityNamesQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}