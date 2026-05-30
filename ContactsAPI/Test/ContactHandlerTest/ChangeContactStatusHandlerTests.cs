using ContactsAPI.Application.Contacts.Commands.ChangeContactStatus;
using ContactsAPI.Entities;
using ContactsAPI.Test.ContactHandlerTest.Helpers;
using Xunit;

namespace ContactsAPI.Test.ContactHandlerTest;

public class ChangeContactStatusHandlerTests
{
    [Theory]
    [InlineData(ContactStatus.Inactive)]
    [InlineData(ContactStatus.Archived)]
    public async Task Handle_WithExistingContact_UpdatesStatusCorrectly(ContactStatus newStatus)
    {
        var dbName = $"{nameof(Handle_WithExistingContact_UpdatesStatusCorrectly)}_{newStatus}";
        await using var context = DbFactory.Create(dbName);

        var contact = new Contact { Name = "Status Test", Email = "status@example.com", Status = ContactStatus.Active };
        context.Contacts.Add(contact);
        await context.SaveChangesAsync();

        var handler = new ChangeContactStatusHandler(context);
        var command = new ChangeContactStatusCommand { Id = contact.Id, Status = newStatus };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result);

        var updated = await context.Contacts.FindAsync(contact.Id);
        Assert.Equal(newStatus, updated!.Status);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsFalse()
    {
        await using var context = DbFactory.Create(nameof(Handle_WithNonExistentId_ReturnsFalse) + "_Status");
        var handler = new ChangeContactStatusHandler(context);
        var command = new ChangeContactStatusCommand { Id = 9999, Status = ContactStatus.Archived };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result);
    }
}
