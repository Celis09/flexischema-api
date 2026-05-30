using ContactsAPI.Application.Admins.Queries.GetAdminConfigs;
using ContactsAPI.Entities;
using ContactsAPI.Test.AdminTestHandler.Helpers;
using Xunit;

namespace ContactsAPI.Test.AdminTestHandler;

public class GetAdminConfigsHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsAllConfigsAsDto()
    {
        await using var context = AdminDbFactory.Create(
            nameof(Handle_ReturnsAllConfigsAsDto));

        context.AdminConfigs.AddRange(
            new AdminConfig { Key = "MaxExtraFieldsPerContact", Value = "5", Description = "Max extra fields" },
            new AdminConfig { Key = "AllowPublicExport", Value = "true", Description = "Allow CSV export" }
        );
        await context.SaveChangesAsync();

        var handler = new GetAdminConfigsHandler(context);
        var result = await handler.Handle(new GetAdminConfigsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Key == "MaxExtraFieldsPerContact" && c.Value == "5");
        Assert.Contains(result, c => c.Key == "AllowPublicExport" && c.Value == "true");
    }

    [Fact]
    public async Task Handle_WithNoConfigs_ReturnsEmptyList()
    {
        await using var context = AdminDbFactory.Create(
            nameof(Handle_WithNoConfigs_ReturnsEmptyList));

        var handler = new GetAdminConfigsHandler(context);
        var result = await handler.Handle(new GetAdminConfigsQuery(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}