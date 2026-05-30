using ContactsAPI.Application.Abstractions;
using ContactsAPI.Application.Contacts.Dtos;
using ContactsAPI.Data;
using ContactsAPI.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ContactsAPI.Test.Validators;

public static class ValidatorFixture
{
    /// <summary>In-memory context with a unique DB per test.</summary>
    public static ContactsDbContext CreateContext(string dbName) =>
        new(new DbContextOptionsBuilder<ContactsDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options);

    /// <summary>
    ///  Config service that returns a fixed max-extra-fields cap (default 5)
    ///  and no other configuration.
    /// </summary>
    public static IConfigService MockConfig(int maxExtraFields = 5)
    {
        var mock = new Mock<IConfigService>();
        mock.Setup(s => s.GetIntAsync("MaxExtraFieldsPerContact", It.IsAny<CancellationToken>()))
            .ReturnsAsync(maxExtraFields);
        return mock.Object;
    }

    /// <summary>Extra-field validator that approves every field unconditionally.</summary>
    public static IValidator<ContactExtraFieldRequest> PassthroughFieldValidator()
    {
        var mock = new Mock<IValidator<ContactExtraFieldRequest>>();
        mock.Setup(v => v.ValidateAsync(
                It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        return mock.Object;
    }
}
