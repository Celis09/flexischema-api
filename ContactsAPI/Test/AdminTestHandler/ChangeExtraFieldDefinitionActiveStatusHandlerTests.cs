using ContactsAPI.Application.Admins.Commands.ChangeExtraFieldDefinitionActiveStatus;
using ContactsAPI.Entities;
using ContactsAPI.Models;
using ContactsAPI.Test.AdminTestHandler.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace ContactsAPI.Test.AdminTestHandler;

public class ChangeExtraFieldDefinitionActiveStatusHandlerTests
{
    [Theory]
    [InlineData(true, false)]   // deactivate an active definition
    [InlineData(false, true)]    // activate an inactive definition
    public async Task Handle_WithExistingDefinition_TogglesIsActiveCorrectly(
        bool initialActive, bool targetActive)
    {
        var dbName = $"{nameof(Handle_WithExistingDefinition_TogglesIsActiveCorrectly)}_{initialActive}_{targetActive}";
        await using var context = AdminDbFactory.Create(dbName);

        var def = new ExtraFieldDefinition
        {
            FieldName = "Twitter",
            FieldType = ExtraFieldType.Url,
            IsActive = initialActive
        };
        context.ExtraFieldDefinitions.Add(def);
        await context.SaveChangesAsync();

        var handler = new ChangeExtraFieldDefinitionActiveStatusHandler(context, new MemoryCache(new MemoryCacheOptions()));
        var command = new ChangeExtraFieldDefinitionActiveStatusCommand
        {
            ExtraFieldDefinitionId = def.ExtraFieldDefinitionId,
            IsActive = targetActive
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);

        var updated = await context.ExtraFieldDefinitions.FindAsync(def.ExtraFieldDefinitionId);
        Assert.Equal(targetActive, updated!.IsActive);
    }

    [Fact]
    public async Task Handle_WithNonExistentDefinition_ReturnsFalse()
    {
        await using var context = AdminDbFactory.Create(
            nameof(Handle_WithNonExistentDefinition_ReturnsFalse) + "_Active");

        var handler = new ChangeExtraFieldDefinitionActiveStatusHandler(context, new MemoryCache(new MemoryCacheOptions()));
        var command = new ChangeExtraFieldDefinitionActiveStatusCommand
        {
            ExtraFieldDefinitionId = 9999,
            IsActive = false
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
    }
}
