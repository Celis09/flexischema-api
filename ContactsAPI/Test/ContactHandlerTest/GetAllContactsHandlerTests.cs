using ContactsAPI.Application.Contacts.Queries.GetContactsPaged;
using ContactsAPI.Entities;
using ContactsAPI.Test.ContactHandlerTest.Helpers;
using Xunit;

namespace ContactsAPI.Test.ContactHandlerTest;

public class GetAllContactsHandlerTests
{
    private static readonly int[] ExpectedPage2Sequences = [4, 5, 6];

    // ── Pagination ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ReturnsCorrectPageMetadata()
    {
        await using var ctx = DbFactory.Create(nameof(Handle_ReturnsCorrectPageMetadata));

        ctx.Contacts.AddRange(Enumerable.Range(1, 15).Select(i =>
            new Contact { Name = $"Contact {i:D2}", Email = $"c{i}@example.com" }));
        await ctx.SaveChangesAsync();

        var handler = new GetAllContactsHandler(ctx);
        var query = new GetAllContactsQuery(Search: null, Page: 2, PageSize: 5, IsAdmin: true);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(15, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(5, result.Items.Count);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasPrev);
        Assert.True(result.HasNext);
    }

    [Fact]
    public async Task Handle_SequenceNumbersReflectCorrectPageOffset()
    {
        await using var ctx = DbFactory.Create(nameof(Handle_SequenceNumbersReflectCorrectPageOffset));

        ctx.Contacts.AddRange(Enumerable.Range(1, 10).Select(i =>
            new Contact { Name = $"User {i:D2}", Email = $"u{i}@example.com" }));
        await ctx.SaveChangesAsync();

        var handler = new GetAllContactsHandler(ctx);

        // Act — page 2 with pageSize 3 → global sequence starts at 4
        var result = await handler.Handle(
            new GetAllContactsQuery(Search: null, Page: 2, PageSize: 3, IsAdmin: true),
            CancellationToken.None);

        // Assert
        var sequences = result.Items.Select(c => c.Sequence).ToList();
        Assert.Equal(ExpectedPage2Sequences, sequences);
    }

    [Fact]
    public async Task Handle_LastPage_ReturnsOnlyRemainingItems()
    {
        await using var ctx = DbFactory.Create(nameof(Handle_LastPage_ReturnsOnlyRemainingItems));

        ctx.Contacts.AddRange(Enumerable.Range(1, 7).Select(i =>
            new Contact { Name = $"Person {i}", Email = $"p{i}@example.com" }));
        await ctx.SaveChangesAsync();

        var handler = new GetAllContactsHandler(ctx);

        // Act — page 3 of pageSize 3 → only 1 item left (7 - 6 = 1)
        var result = await handler.Handle(
            new GetAllContactsQuery(Search: null, Page: 3, PageSize: 3, IsAdmin: true),
            CancellationToken.None);

        // Assert
        Assert.Single(result.Items);
        Assert.False(result.HasNext);
    }

    // ── Role-based visibility ─────────────────────────────────────────────────

    [Fact]
    public async Task Handle_AsAdmin_ReturnsAllStatusContacts()
    {
        await using var ctx = DbFactory.Create(nameof(Handle_AsAdmin_ReturnsAllStatusContacts));

        ctx.Contacts.AddRange(
            new Contact { Name = "Active", Email = "active@example.com", Status = ContactStatus.Active },
            new Contact { Name = "Inactive", Email = "inactive@example.com", Status = ContactStatus.Inactive },
            new Contact { Name = "Archived", Email = "archived@example.com", Status = ContactStatus.Archived }
        );
        await ctx.SaveChangesAsync();

        var handler = new GetAllContactsHandler(ctx);
        var result = await handler.Handle(
            new GetAllContactsQuery(Search: null, IsAdmin: true), CancellationToken.None);

        // Admin should see all 3
        Assert.Equal(3, result.TotalCount);
    }

    [Fact]
    public async Task Handle_AsEditor_ReturnsOnlyActiveContacts()
    {
        await using var ctx = DbFactory.Create(nameof(Handle_AsEditor_ReturnsOnlyActiveContacts));

        ctx.Contacts.AddRange(
            new Contact { Name = "Active", Email = "active@example.com", Status = ContactStatus.Active },
            new Contact { Name = "Inactive", Email = "inactive@example.com", Status = ContactStatus.Inactive },
            new Contact { Name = "Archived", Email = "archived@example.com", Status = ContactStatus.Archived }
        );
        await ctx.SaveChangesAsync();

        var handler = new GetAllContactsHandler(ctx);
        var result = await handler.Handle(
            new GetAllContactsQuery(Search: null, IsAdmin: false, IsEditor: true), CancellationToken.None);

        // Editor sees Active only
        Assert.Equal(1, result.TotalCount);
        Assert.All(result.Items, c => Assert.Equal("Active", c.Status));
    }

    [Fact]
    public async Task Handle_AsPublic_ReturnsOnlyActiveContacts()
    {
        await using var ctx = DbFactory.Create(nameof(Handle_AsPublic_ReturnsOnlyActiveContacts));

        ctx.Contacts.AddRange(
            new Contact { Name = "Active", Email = "active@example.com", Status = ContactStatus.Active },
            new Contact { Name = "Archived", Email = "archived@example.com", Status = ContactStatus.Archived }
        );
        await ctx.SaveChangesAsync();

        var handler = new GetAllContactsHandler(ctx);
        var result = await handler.Handle(
            new GetAllContactsQuery(Search: null, IsAdmin: false, IsEditor: false), CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal("Active", result.Items[0].Status);
    }

    // ── Status filter ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithStatusFilter_ReturnsOnlyMatchingContacts()
    {
        await using var ctx = DbFactory.Create(nameof(Handle_WithStatusFilter_ReturnsOnlyMatchingContacts));

        ctx.Contacts.AddRange(
            new Contact { Name = "A1", Email = "a1@example.com", Status = ContactStatus.Active },
            new Contact { Name = "A2", Email = "a2@example.com", Status = ContactStatus.Active },
            new Contact { Name = "I1", Email = "i1@example.com", Status = ContactStatus.Inactive }
        );
        await ctx.SaveChangesAsync();

        var handler = new GetAllContactsHandler(ctx);
        var result = await handler.Handle(
            new GetAllContactsQuery(Search: null, Status: ContactStatus.Inactive, IsAdmin: true),
            CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.All(result.Items, c => Assert.Equal("Inactive", c.Status));
    }

    // ── Date range filter ─────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithFromDateFilter_ExcludesContactsCreatedBefore()
    {
        await using var ctx = DbFactory.Create(nameof(Handle_WithFromDateFilter_ExcludesContactsCreatedBefore));

        var cutoff = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        ctx.Contacts.AddRange(
            new Contact { Name = "Old", Email = "old@example.com", CreatedDate = cutoff.AddDays(-5) },
            new Contact { Name = "OnDay", Email = "onday@example.com", CreatedDate = cutoff },
            new Contact { Name = "New", Email = "new@example.com", CreatedDate = cutoff.AddDays(2) }
        );
        await ctx.SaveChangesAsync();

        var handler = new GetAllContactsHandler(ctx);
        var result = await handler.Handle(
            new GetAllContactsQuery(Search: null, FromDate: cutoff, IsAdmin: true),
            CancellationToken.None);

        // Should include OnDay and New, but not Old
        Assert.Equal(2, result.TotalCount);
        Assert.DoesNotContain(result.Items, c => c.Name == "Old");
    }

    [Fact]
    public async Task Handle_WithToDateFilter_ExcludesContactsCreatedAfter()
    {
        await using var ctx = DbFactory.Create(nameof(Handle_WithToDateFilter_ExcludesContactsCreatedAfter));

        var cutoff = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        ctx.Contacts.AddRange(
            new Contact { Name = "Early", Email = "early@example.com", CreatedDate = cutoff.AddDays(-1) },
            new Contact { Name = "OnDay", Email = "onday@example.com", CreatedDate = cutoff },
            new Contact { Name = "Future", Email = "future@example.com", CreatedDate = cutoff.AddDays(1) }
        );
        await ctx.SaveChangesAsync();

        var handler = new GetAllContactsHandler(ctx);
        var result = await handler.Handle(
            new GetAllContactsQuery(Search: null, ToDate: cutoff, IsAdmin: true),
            CancellationToken.None);

        // ToDate is inclusive of its day (handler adds 1 day internally)
        Assert.Equal(2, result.TotalCount);
        Assert.DoesNotContain(result.Items, c => c.Name == "Future");
    }

    [Fact]
    public async Task Handle_WithFromAndToDateFilter_ReturnsContactsInRange()
    {
        await using var ctx = DbFactory.Create(nameof(Handle_WithFromAndToDateFilter_ReturnsContactsInRange));

        var from = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc);

        ctx.Contacts.AddRange(
            new Contact { Name = "Before", Email = "before@example.com", CreatedDate = from.AddDays(-1) },
            new Contact { Name = "InRange", Email = "inrange@example.com", CreatedDate = from.AddDays(5) },
            new Contact { Name = "After", Email = "after@example.com", CreatedDate = to.AddDays(2) }
        );
        await ctx.SaveChangesAsync();

        var handler = new GetAllContactsHandler(ctx);
        var result = await handler.Handle(
            new GetAllContactsQuery(Search: null, FromDate: from, ToDate: to, IsAdmin: true),
            CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal("InRange", result.Items[0].Name);
    }

    // ── Empty result ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithNoContacts_ReturnsEmptyPagedResult()
    {
        await using var ctx = DbFactory.Create(nameof(Handle_WithNoContacts_ReturnsEmptyPagedResult));
        var handler = new GetAllContactsHandler(ctx);

        var result = await handler.Handle(
            new GetAllContactsQuery(Search: null, IsAdmin: true), CancellationToken.None);

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalPages);
    }
}