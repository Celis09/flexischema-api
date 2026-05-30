using ContactsAPI.Application.Admins.Queries.GetExtraFieldDefinitions;
using ContactsAPI.Entities;
using ContactsAPI.Models;
using ContactsAPI.Test.AdminTestHandler.Helpers;
using Xunit;

namespace ContactsAPI.Test.AdminTestHandler;

public class GetExtraFieldDefinitionsHandlerTest
{
    [Fact]
    public async Task Handle_ShouldReturnFilteredResults()
    {
        await using var context = AdminDbFactory.Create(
            nameof(Handle_ShouldReturnFilteredResults));

        context.ExtraFieldDefinitions.AddRange(
            new ExtraFieldDefinition { FieldName = "Twitter", FieldType = ExtraFieldType.Text, IsActive = true },
            new ExtraFieldDefinition { FieldName = "SecretNotes", FieldType = ExtraFieldType.Text, IsActive = false }
        );
        await context.SaveChangesAsync();

        var handler = new GetExtraFieldDefinitionsHandler(context);

        var query = new GetExtraFieldDefinitionsQuery { IsActive = true };

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Twitter", result[0].FieldName);
    }

    [Fact]
    public async Task Handle_WithNoDefinitions_ReturnsEmptyList()
    {
        await using var context = AdminDbFactory.Create(
            nameof(Handle_WithNoDefinitions_ReturnsEmptyList));

        var handler = new GetExtraFieldDefinitionsHandler(context);
        var result = await handler.Handle(
            new GetExtraFieldDefinitionsQuery { IsActive = true }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}