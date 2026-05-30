using ContactsAPI.Application.Contacts.Queries.GetContactById;
using ContactsAPI.Entities;
using ContactsAPI.Models;
using ContactsAPI.Test.ContactHandlerTest.Helpers;
using Xunit;

namespace ContactsAPI.Test.ContactHandlerTest;

public class GetContactByIdHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingId_ReturnsCorrectContactDto()
    {
        await using var context = DbFactory.Create(nameof(Handle_WithExistingId_ReturnsCorrectContactDto));

        var def = new ExtraFieldDefinition { FieldName = "Website", FieldType = ExtraFieldType.Url };
        context.ExtraFieldDefinitions.Add(def);
        await context.SaveChangesAsync();

        var contact = new Contact
        {
            Name = "Bob Builder",
            Email = "bob@example.com",
            ExtraFields = new List<ContactExtraField>
            {
                new() { ExtraFieldDefinitionId = def.ExtraFieldDefinitionId, FieldValue = "bob.dev" }
            }
        };
        context.Contacts.Add(contact);
        await context.SaveChangesAsync();

        var handler = new GetContactByIdHandler(context);

        var dto = await handler.Handle(new GetContactByIdQuery { Id = contact.Id }, CancellationToken.None);

        Assert.NotNull(dto);
        Assert.Equal(contact.Id, dto.Id);
        Assert.Equal("Bob Builder", dto.Name);
        Assert.Equal("bob@example.com", dto.Email);
        Assert.Single(dto.ExtraFields);
        Assert.Equal("Website", dto.ExtraFields[0].FieldName);
        Assert.Equal("bob.dev", dto.ExtraFields[0].FieldValue);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsNull()
    {
        await using var context = DbFactory.Create(nameof(Handle_WithNonExistentId_ReturnsNull));
        var handler = new GetContactByIdHandler(context);

        var dto = await handler.Handle(new GetContactByIdQuery { Id = 9999 }, CancellationToken.None);

        Assert.Null(dto);
    }
}
