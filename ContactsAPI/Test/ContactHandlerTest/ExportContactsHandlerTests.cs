using ContactsAPI.Application.Contacts.Queries.ExportContacts;
using ContactsAPI.Entities;
using ContactsAPI.Services;
using ContactsAPI.Test.ContactHandlerTest.Helpers;
using Xunit;

namespace ContactsAPI.Test.ContactHandlerTest;

public class ExportContactsHandlerTests
{
    // We instantiate the concrete service here to pass into the handler.
    private readonly IContactExportService _exportService = new ContactExportService();

    // ── No contacts ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithNoContacts_ReturnsEmptyMessage()
    {
        await using var ctx = DbFactory.Create(nameof(Handle_WithNoContacts_ReturnsEmptyMessage));

        var result = await new ExportContactsHandler(ctx, _exportService).Handle(
            new ExportContactsQuery("csv", IsAdmin: true), CancellationToken.None);

        Assert.Equal("text/plain", result.ContentType);
        Assert.Contains("No contacts", result.Content);
    }

    // ── CSV format ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_CsvFormat_ContainsHeaderAndDataRows()
    {
        await using var ctx = DbFactory.Create(nameof(Handle_CsvFormat_ContainsHeaderAndDataRows));

        ctx.Contacts.AddRange(
            new Contact { Name = "Alice", Email = "alice@example.com", Status = ContactStatus.Active },
            new Contact { Name = "Bob", Email = "bob@example.com", Status = ContactStatus.Active }
        );
        await ctx.SaveChangesAsync();

        var result = await new ExportContactsHandler(ctx, _exportService).Handle(
            new ExportContactsQuery("csv", IsAdmin: true), CancellationToken.None);

        Assert.Equal("text/csv", result.ContentType);
        Assert.Contains("Name", result.Content);
        Assert.Contains("Email", result.Content);
        Assert.Contains("Alice", result.Content);
        Assert.Contains("Bob", result.Content);
    }

    // ── JSON format ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_JsonFormat_ReturnsValidJson()
    {
        await using var ctx = DbFactory.Create(nameof(Handle_JsonFormat_ReturnsValidJson));

        ctx.Contacts.Add(new Contact { Name = "Carol", Email = "carol@example.com" });
        await ctx.SaveChangesAsync();

        var result = await new ExportContactsHandler(ctx, _exportService).Handle(
            new ExportContactsQuery("json", IsAdmin: true), CancellationToken.None);

        Assert.Equal("application/json", result.ContentType);
        Assert.Contains("Carol", result.Content);
    }

    // ── Role-based column filtering ───────────────────────────────────────────

    [Fact]
    public async Task Handle_AsPublic_CsvDoesNotContainStatusOrId()
    {
        await using var ctx = DbFactory.Create(nameof(Handle_AsPublic_CsvDoesNotContainStatusOrId));

        ctx.Contacts.Add(new Contact { Name = "Dave", Email = "dave@example.com" });
        await ctx.SaveChangesAsync();

        var result = await new ExportContactsHandler(ctx, _exportService).Handle(
            new ExportContactsQuery("csv", IsAdmin: false, IsEditor: false), CancellationToken.None);

        // Public role: only Name and Email columns are allowed
        Assert.DoesNotContain("Status", result.Content);
        Assert.DoesNotContain("Created Date", result.Content);
        Assert.Contains("Name", result.Content);
        Assert.Contains("Email", result.Content);
    }

    [Fact]
    public async Task Handle_AsAdmin_CsvContainsStatusAndCreatedDate()
    {
        await using var ctx = DbFactory.Create(nameof(Handle_AsAdmin_CsvContainsStatusAndCreatedDate));

        ctx.Contacts.Add(new Contact { Name = "Eve", Email = "eve@example.com" });
        await ctx.SaveChangesAsync();

        var result = await new ExportContactsHandler(ctx, _exportService).Handle(
            new ExportContactsQuery("csv", IsAdmin: true), CancellationToken.None);

        Assert.Contains("Status", result.Content);
        Assert.Contains("Created Date", result.Content);
    }

    // ── ID filter ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithIdsFilter_ReturnsOnlySelectedContacts()
    {
        await using var ctx = DbFactory.Create(nameof(Handle_WithIdsFilter_ReturnsOnlySelectedContacts));

        ctx.Contacts.AddRange(
            new Contact { Name = "Frank", Email = "frank@example.com" },
            new Contact { Name = "Grace", Email = "grace@example.com" }
        );
        await ctx.SaveChangesAsync();

        var frank = ctx.Contacts.First(c => c.Name == "Frank");

        var result = await new ExportContactsHandler(ctx, _exportService).Handle(
            new ExportContactsQuery("csv", Ids: frank.Id.ToString(), IsAdmin: true),
            CancellationToken.None);

        Assert.Contains("Frank", result.Content);
        Assert.DoesNotContain("Grace", result.Content);
    }
}