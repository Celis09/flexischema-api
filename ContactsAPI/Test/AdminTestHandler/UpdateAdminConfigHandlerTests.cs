using ContactsAPI.Application.Admins.Commands.UpdateAdminConfig;
using ContactsAPI.Application.Exceptions;
using ContactsAPI.Entities;
using ContactsAPI.Test.AdminTestHandler.Helpers;
using Xunit;

namespace ContactsAPI.Test.AdminTestHandler;

public class UpdateAdminConfigHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingConfig_UpdatesValueAndReturnsId()
    {
        await using var context = AdminDbFactory.Create(
            nameof(Handle_WithExistingConfig_UpdatesValueAndReturnsId));

        var config = new AdminConfig
        {
            Key = "MaxExtraFieldsPerContact",
            Value = "5",
            Description = "Maximum number of extra fields allowed per contact"
        };
        context.AdminConfigs.Add(config);
        await context.SaveChangesAsync();

        var handler = new UpdateAdminConfigHandler(context);
        var command = new UpdateAdminConfigCommand { Id = config.Id, Value = "10" };

        var returnedId = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(config.Id, returnedId);

        var updated = await context.AdminConfigs.FindAsync(config.Id);
        Assert.Equal("10", updated!.Value);
    }

    [Fact]
    public async Task Handle_WithNonExistentConfig_ThrowsNotFoundException()
    {
        await using var context = AdminDbFactory.Create(
            nameof(Handle_WithNonExistentConfig_ThrowsNotFoundException));

        var handler = new UpdateAdminConfigHandler(context);
        var command = new UpdateAdminConfigCommand { Id = 9999, Value = "99" };

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
    }
}