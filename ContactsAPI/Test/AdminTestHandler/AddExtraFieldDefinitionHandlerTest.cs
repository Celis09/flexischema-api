using ContactsAPI.Application.Admins.Commands.AddExtraFieldDefinition;
using ContactsAPI.Models;
using ContactsAPI.Test.AdminTestHandler.Helpers;
using Xunit;

namespace ContactsAPI.Test.AdminTestHandler;

public class AddExtraFieldDefinitionHandlerTest
{
    [Fact]
    public async Task Handle_ShouldCreateDefinition()
    {
        await using var context = AdminDbFactory.Create(
            nameof(Handle_ShouldCreateDefinition));

        var handler = new AddExtraFieldDefinitionHandler(context);
        var command = new AddExtraFieldDefinitionCommand
        {
            FieldName = "Twitter",
            FieldType = ExtraFieldType.Text,
            IsActive = true,
        };

        var id = await handler.Handle(command, CancellationToken.None);

        var definition = await context.ExtraFieldDefinitions.FindAsync(id);
        Assert.NotNull(definition);
        Assert.Equal("Twitter", definition.FieldName);
        Assert.Equal(ExtraFieldType.Text, definition.FieldType);
        Assert.True(definition.IsActive);
    }
}