using ContactsAPI.Application.Admins.Commands.ChangeExtraFieldDefinitionRequiredStatus;
using ContactsAPI.Entities;
using ContactsAPI.Models;
using ContactsAPI.Test.AdminTestHandler.Helpers;
using Xunit;

namespace ContactsAPI.Test.AdminTestHandler;

public class ChangeExtraFieldDefinitionRequiredStatusHandlerTests
{
    [Theory]
    [InlineData(false, true)]    // mark as required
    [InlineData(true, false)]   // mark as optional
    public async Task Handle_WithExistingDefinition_TogglesIsRequiredCorrectly(
        bool initialRequired, bool targetRequired)
    {
        var dbName = $"{nameof(Handle_WithExistingDefinition_TogglesIsRequiredCorrectly)}_{initialRequired}_{targetRequired}";
        await using var context = AdminDbFactory.Create(dbName);

        var def = new ExtraFieldDefinition
        {
            FieldName = "Phone",
            FieldType = ExtraFieldType.Phone,
            IsRequired = initialRequired
        };
        context.ExtraFieldDefinitions.Add(def);
        await context.SaveChangesAsync();

        var handler = new ChangeExtraFieldDefinitionRequiredStatusHandler(context);
        var command = new ChangeExtraFieldDefinitionRequiredStatusCommand
        {
            ExtraFieldDefinitionId = def.ExtraFieldDefinitionId,
            IsRequired = targetRequired
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);

        var updated = await context.ExtraFieldDefinitions.FindAsync(def.ExtraFieldDefinitionId);
        Assert.Equal(targetRequired, updated!.IsRequired);
    }

    [Fact]
    public async Task Handle_WithNonExistentDefinition_ReturnsFalse()
    {
        await using var context = AdminDbFactory.Create(
            nameof(Handle_WithNonExistentDefinition_ReturnsFalse) + "_Required");

        var handler = new ChangeExtraFieldDefinitionRequiredStatusHandler(context);
        var command = new ChangeExtraFieldDefinitionRequiredStatusCommand
        {
            ExtraFieldDefinitionId = 9999,
            IsRequired = true
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
    }
}
