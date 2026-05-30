using ContactsAPI.Application.Contacts.Commands.UpdateContact;
using ContactsAPI.Application.Contacts.Dtos;
using ContactsAPI.Entities;
using ContactsAPI.Models;
using ContactsAPI.Test.ContactHandlerTest.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ContactsAPI.Test.ContactHandlerTest;

public class UpdateContactHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingContact_UpdatesNameEmailAndExtraFields()
    {
        await using var context = DbFactory.Create(nameof(Handle_WithExistingContact_UpdatesNameEmailAndExtraFields));

        var twitterDef = new ExtraFieldDefinition { FieldName = "Twitter", FieldType = ExtraFieldType.Url };
        var linkedinDef = new ExtraFieldDefinition { FieldName = "LinkedIn", FieldType = ExtraFieldType.Url };
        context.ExtraFieldDefinitions.AddRange(twitterDef, linkedinDef);
        await context.SaveChangesAsync();

        var contact = new Contact
        {
            Name = "John Doe",
            Email = "john@example.com",
            ExtraFields = new List<ContactExtraField>
            {
                new() { ExtraFieldDefinitionId = twitterDef.ExtraFieldDefinitionId, FieldValue = "@john_old" }
            }
        };
        context.Contacts.Add(contact);
        await context.SaveChangesAsync();

        var handler = new UpdateContactHandler(context);

        var command = new UpdateContactCommand
        {
            Id = contact.Id,
            Name = "John Updated",
            Email = "john.updated@example.com",
            ExtraFields = new List<ContactExtraFieldRequest>
            {
                new() { ExtraFieldDefinitionId = twitterDef.ExtraFieldDefinitionId,  FieldValue = "@john_new" },
                new() { ExtraFieldDefinitionId = linkedinDef.ExtraFieldDefinitionId, FieldValue = "linkedin.com/in/john" }
            }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result);

        var updated = await context.Contacts
            .Include(c => c.ExtraFields)
            .FirstAsync(c => c.Id == contact.Id);

        Assert.Equal("John Updated", updated.Name);
        Assert.Equal("john.updated@example.com", updated.Email);
        Assert.Equal(2, updated.ExtraFields.Count);
        Assert.Contains(updated.ExtraFields, f => f.FieldValue == "@john_new");
        Assert.Contains(updated.ExtraFields, f => f.FieldValue == "linkedin.com/in/john");
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsFalse()
    {
        await using var context = DbFactory.Create(nameof(Handle_WithNonExistentId_ReturnsFalse));
        var handler = new UpdateContactHandler(context);

        var command = new UpdateContactCommand
        {
            Id = 9999,
            Name = "Ghost",
            Email = "ghost@example.com",
            ExtraFields = new List<ContactExtraFieldRequest>()
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result);
    }
}
