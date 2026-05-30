using ContactsAPI.Application.Admins.Commands.UpdateExtraFieldDefinition;
using ContactsAPI.Entities;
using ContactsAPI.Models;
using ContactsAPI.Test.AdminTestHandler.Helpers;
using Xunit;

namespace ContactsAPI.Test.AdminTestHandler;

public class UpdateExtraFieldDefinitionHandlerTest
{
    [Fact]
    public async Task Handle_ShouldModifyDefinition()
    {
        await using var context = AdminDbFactory.Create(
            nameof(Handle_ShouldModifyDefinition));

        var def = new ExtraFieldDefinition
        {
            FieldName = "Twitter",
            FieldType = ExtraFieldType.Text,
            IsActive = true
        };
        context.ExtraFieldDefinitions.Add(def);
        await context.SaveChangesAsync();

        var handler = new UpdateExtraFieldDefinitionHandler(context);
        var command = new UpdateExtraFieldDefinitionCommand
        {
            ExtraFieldDefinitionId = def.ExtraFieldDefinitionId,
            FieldName = "LinkedIn",
            FieldType = ExtraFieldType.Url,   // deliberately changing type
            IsActive = false,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result);
        var updated = await context.ExtraFieldDefinitions.FindAsync(def.ExtraFieldDefinitionId);
        Assert.Equal("LinkedIn", updated!.FieldName);
        Assert.Equal(ExtraFieldType.Url, updated.FieldType);  // ✅ was "Profile" (non-existent)
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsFalse()
    {
        await using var context = AdminDbFactory.Create(
            nameof(Handle_WithNonExistentId_ReturnsFalse));

        var handler = new UpdateExtraFieldDefinitionHandler(context);
        var command = new UpdateExtraFieldDefinitionCommand
        {
            ExtraFieldDefinitionId = 9999,
            FieldName = "Ghost",
            FieldType = ExtraFieldType.Text
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result);
    }
}