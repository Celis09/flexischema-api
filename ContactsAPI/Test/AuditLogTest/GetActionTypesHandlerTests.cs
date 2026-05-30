using ContactsAPI.Application.AuditLogs.Queries.GetAllAuditLogs;
using ContactsAPI.Data;
using ContactsAPI.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ContactsAPI.Test.AuditLogTest;

public class GetActionTypesHandlerTests
{
    private static readonly string[] ExpectedActionTypes = ["CreateContact", "DeleteContact"];

    private static ContactsDbContext Ctx(string name) =>
        new(new DbContextOptionsBuilder<ContactsDbContext>()
            .UseInMemoryDatabase(name)
            .Options);

    [Fact]
    public async Task Handle_ReturnsDistinctActionTypesSorted()
    {
        await using var ctx = Ctx(nameof(Handle_ReturnsDistinctActionTypesSorted));
        ctx.AuditLogs.AddRange(
            new AuditLog { ActionType = "DeleteContact", EntityName = "Contact", EntityId = "1" },
            new AuditLog { ActionType = "CreateContact", EntityName = "Contact", EntityId = "2" },
            new AuditLog { ActionType = "CreateContact", EntityName = "Contact", EntityId = "3" }  // duplicate
        );
        await ctx.SaveChangesAsync();

        var result = await new GetActionTypesHandler(ctx).Handle(
            new GetActionTypesQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(ExpectedActionTypes, result.ToArray()); // sorted asc
    }

    [Fact]
    public async Task Handle_WithNoLogs_ReturnsEmptyList()
    {
        await using var ctx = Ctx(nameof(Handle_WithNoLogs_ReturnsEmptyList));

        var result = await new GetActionTypesHandler(ctx).Handle(
            new GetActionTypesQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}