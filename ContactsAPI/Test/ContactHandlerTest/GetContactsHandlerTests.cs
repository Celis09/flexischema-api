using ContactsAPI.Application.Contacts.Queries.GetContactsPaged;
using ContactsAPI.Entities;
using ContactsAPI.Test.ContactHandlerTest.Helpers;
using Xunit;

namespace ContactsAPI.Test.ContactHandlerTest;

public class GetContactsHandlerTests
{
    private static readonly int[] ExpectedSequences = [1, 2, 3];

    [Fact]
    public async Task Handle_WithMultipleContacts_ReturnsAllContactsWithSequence()
    {
        await using var context = DbFactory.Create(nameof(Handle_WithMultipleContacts_ReturnsAllContactsWithSequence));
        context.Contacts.AddRange(
            new Contact { Name = "Alice", Email = "alice@example.com" },
            new Contact { Name = "Bob", Email = "bob@example.com" },
            new Contact { Name = "Carol", Email = "carol@example.com" }
        );
        await context.SaveChangesAsync();

        var handler = new GetContactsHandler(context);
        var result = await handler.Handle(new GetContactsQuery(), CancellationToken.None);

        Assert.Equal(3, result.Count);
        Assert.Equal(ExpectedSequences, result.Select(c => c.Sequence));
        Assert.Contains(result, c => c.Name == "Alice");
        Assert.Contains(result, c => c.Name == "Bob");
        Assert.Contains(result, c => c.Name == "Carol");
    }

    [Fact]
    public async Task Handle_WithNoContacts_ReturnsEmptyList()
    {
        await using var context = DbFactory.Create(nameof(Handle_WithNoContacts_ReturnsEmptyList));

        var handler = new GetContactsHandler(context);
        var result = await handler.Handle(new GetContactsQuery(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}