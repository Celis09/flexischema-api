using ContactsAPI.Application.Contacts.Commands.CreateContact;
using ContactsAPI.Application.Contacts.Dtos;
using ContactsAPI.Entities;
using ContactsAPI.Models;
using ContactsAPI.Test.ContactHandlerTest.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ContactsAPI.Test.ContactHandlerTest;

public class CreateContactHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_PersistsContactAndReturnsId()
    {
        await using var context = DbFactory.Create(nameof(Handle_WithValidCommand_PersistsContactAndReturnsId));
        var handler = new CreateContactHandler(context);

        var command = new CreateContactCommand
        {
            Name = "Jane Doe",
            Email = "jane@example.com",
            ExtraFields = new List<ContactExtraFieldRequest>()
        };

        var id = await handler.Handle(command, CancellationToken.None);

        Assert.True(id > 0);

        var saved = await context.Contacts.FindAsync(id);
        Assert.NotNull(saved);
        Assert.Equal("Jane Doe", saved.Name);
        Assert.Equal("jane@example.com", saved.Email);
        Assert.Equal(ContactStatus.Active, saved.Status);
    }

    [Fact]
    public async Task Handle_WithExtraFields_PersistsExtraFieldsCorrectly()
    {
        await using var context = DbFactory.Create(nameof(Handle_WithExtraFields_PersistsExtraFieldsCorrectly));

        var def = new ExtraFieldDefinition { FieldName = "Twitter", FieldType = ExtraFieldType.Url };
        context.ExtraFieldDefinitions.Add(def);
        await context.SaveChangesAsync();

        var handler = new CreateContactHandler(context);

        var command = new CreateContactCommand
        {
            Name = "Alice",
            Email = "alice@example.com",
            ExtraFields = new List<ContactExtraFieldRequest>
            {
                new() { ExtraFieldDefinitionId = def.ExtraFieldDefinitionId, FieldValue = "@alice" }
            }
        };

        var id = await handler.Handle(command, CancellationToken.None);

        var saved = await context.Contacts
            .Include(c => c.ExtraFields)
            .FirstAsync(c => c.Id == id);

        Assert.Single(saved.ExtraFields);
        Assert.Equal("@alice", saved.ExtraFields.First().FieldValue);
    }
}
