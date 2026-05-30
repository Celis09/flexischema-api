using ContactsAPI.Application.Admins.Commands.ImportContacts;
using ContactsAPI.Application.Contacts.Commands.CreateContact;
using ContactsAPI.Entities;
using ContactsAPI.Models;
using ContactsAPI.Test.AdminTestHandler.Helpers;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Xunit;

namespace ContactsAPI.Test.AdminTestHandler;

public class ImportContactsHandlerTests
{
    private static IValidator<CreateContactCommand> PassthroughValidator()
    {
        var mock = new Mock<IValidator<CreateContactCommand>>();

        mock.Setup(v => v.ValidateAsync(
                It.IsAny<CreateContactCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        return mock.Object;
    }

    // ── Empty CSV ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_EmptyCsv_ReturnsAllZeroCounts()
    {
        await using var ctx = AdminDbFactory.Create(nameof(Handle_EmptyCsv_ReturnsAllZeroCounts));

        var handler = new ImportContactsHandler(ctx, PassthroughValidator());
        var result = await handler.Handle(
            new ImportContactsCommand(""), CancellationToken.None);

        Assert.Equal(0, result.ImportedCount);
        Assert.Equal(0, result.UpdatedCount);
        Assert.Equal(0, result.SkippedCount);
    }

    // ── New contact is imported ───────────────────────────────────────────────

    [Fact]
    public async Task Handle_SingleNewContact_ImportsAndPersists()
    {
        await using var ctx = AdminDbFactory.Create(nameof(Handle_SingleNewContact_ImportsAndPersists));

        var nl = Environment.NewLine;
        var csv = $"Name,Email{nl}John Doe,john@example.com";
        var handler = new ImportContactsHandler(ctx, PassthroughValidator());

        var result = await handler.Handle(
            new ImportContactsCommand(csv), CancellationToken.None);

        Assert.Equal(1, result.ImportedCount);
        Assert.Equal(0, result.SkippedCount);

        var saved = ctx.Contacts.FirstOrDefault(c => c.Email == "john@example.com");
        Assert.NotNull(saved);
        Assert.Equal("John Doe", saved!.Name);
    }

    // ── DryRun does not persist ───────────────────────────────────────────────

    [Fact]
    public async Task Handle_DryRun_DoesNotPersistContacts()
    {
        await using var ctx = AdminDbFactory.Create(nameof(Handle_DryRun_DoesNotPersistContacts));

        var nl = Environment.NewLine;
        var csv = $"Name,Email{nl}Dry User,dry@example.com";
        var handler = new ImportContactsHandler(ctx, PassthroughValidator());

        var result = await handler.Handle(
            new ImportContactsCommand(csv, DryRun: true), CancellationToken.None);

        Assert.Equal(1, result.ImportedCount);   // previewed as "New"
        Assert.Empty(ctx.Contacts);              // nothing actually saved
    }

    // ── Existing contact is updated when OverwriteExisting = true ────────────

    [Fact]
    public async Task Handle_ExistingContact_WithOverwrite_UpdatesField()
    {
        await using var ctx = AdminDbFactory.Create(
            nameof(Handle_ExistingContact_WithOverwrite_UpdatesField));

        var def = new ExtraFieldDefinition
        {
            FieldName = "Twitter",
            FieldType = ExtraFieldType.Text,
            IsActive = true
        };
        ctx.ExtraFieldDefinitions.Add(def);

        var existing = new Contact
        {
            Name = "Alice",
            Email = "alice@example.com",
            ExtraFields = new List<ContactExtraField>
            {
                new() { ExtraFieldDefinitionId = def.ExtraFieldDefinitionId, FieldValue = "@old" }
            }
        };
        ctx.Contacts.Add(existing);
        await ctx.SaveChangesAsync();

        var nl = Environment.NewLine;
        var csv = $"Name,Email,Twitter{nl}Alice,alice@example.com,@new";
        var handler = new ImportContactsHandler(ctx, PassthroughValidator());

        var result = await handler.Handle(
            new ImportContactsCommand(csv, OverwriteExisting: true), CancellationToken.None);

        Assert.Equal(1, result.UpdatedCount);

        var updated = ctx.ContactExtraFields
            .First(f => f.ContactId == existing.Id &&
                        f.ExtraFieldDefinitionId == def.ExtraFieldDefinitionId);
        Assert.Equal("@new", updated.FieldValue);
    }

    // ── Auto-create missing definitions ──────────────────────────────────────

    [Fact]
    public async Task Handle_AutoCreateDefinitions_CreatesNewDefinition()
    {
        await using var ctx = AdminDbFactory.Create(
            nameof(Handle_AutoCreateDefinitions_CreatesNewDefinition));

        var nl = Environment.NewLine;
        var csv = $"Name,Email,LinkedIn{nl}Bob,bob@example.com,linkedin.com/in/bob";
        var handler = new ImportContactsHandler(ctx, PassthroughValidator());

        await handler.Handle(
            new ImportContactsCommand(csv, AutoCreateDefinitions: true),
            CancellationToken.None);

        Assert.True(ctx.ExtraFieldDefinitions.Any(d => d.FieldName == "LinkedIn"));
    }

    // ── Missing required field skips the row ─────────────────────────────────

    [Fact]
    public async Task Handle_MissingRequiredField_SkipsRow()
    {
        await using var ctx = AdminDbFactory.Create(nameof(Handle_MissingRequiredField_SkipsRow));

        ctx.ExtraFieldDefinitions.Add(new ExtraFieldDefinition
        {
            FieldName = "Phone",
            FieldType = ExtraFieldType.Phone,
            IsActive = true,
            IsRequired = true
        });
        await ctx.SaveChangesAsync();

        var nl = Environment.NewLine;
        var csv = $"Name,Email{nl}Carol,carol@example.com";
        var handler = new ImportContactsHandler(ctx, PassthroughValidator());

        var result = await handler.Handle(
            new ImportContactsCommand(csv), CancellationToken.None);

        Assert.Equal(0, result.ImportedCount);
        Assert.Equal(1, result.SkippedCount);
        Assert.Contains(result.Errors, e => e.Contains("Missing required field"));
    }
}