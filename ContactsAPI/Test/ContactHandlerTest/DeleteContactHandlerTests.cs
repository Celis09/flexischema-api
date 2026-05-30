using ContactsAPI.Application.Contacts.Commands.DeleteContact;
using ContactsAPI.Entities;
using ContactsAPI.Models;
using ContactsAPI.Test.ContactHandlerTest.Helpers;
using Xunit;

namespace ContactsAPI.Test.ContactHandlerTest;

public class DeleteContactHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingContact_RemovesContactAndExtraFields()
    {
        await using var context = DbFactory.Create(nameof(Handle_WithExistingContact_RemovesContactAndExtraFields));

        var def = new ExtraFieldDefinition { FieldName = "Phone", FieldType = ExtraFieldType.Phone };
        context.ExtraFieldDefinitions.Add(def);
        await context.SaveChangesAsync();

        var contact = new Contact
        {
            Name = "To Delete",
            Email = "delete@example.com",
            ExtraFields = new List<ContactExtraField>
            {
                new() { ExtraFieldDefinitionId = def.ExtraFieldDefinitionId, FieldValue = "+63912345678" }
            }
        };
        context.Contacts.Add(contact);
        await context.SaveChangesAsync();

        var contactId = contact.Id;
        var handler = new DeleteContactHandler(context);

        var result = await handler.Handle(new DeleteContactCommand { Id = contactId }, CancellationToken.None);

        Assert.True(result);
        Assert.Null(await context.Contacts.FindAsync(contactId));
        Assert.Empty(context.ContactExtraFields.Where(f => f.ContactId == contactId));
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsFalse()
    {
        await using var context = DbFactory.Create(nameof(Handle_WithNonExistentId_ReturnsFalse) + "_Delete");
        var handler = new DeleteContactHandler(context);

        var result = await handler.Handle(new DeleteContactCommand { Id = 9999 }, CancellationToken.None);

        Assert.False(result);
    }
}
