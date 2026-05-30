using ContactsAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Test.ContactHandlerTest.Helpers;

public static class DbFactory
{
    /// <summary>Creates an isolated in-memory context for each test.</summary>
    public static ContactsDbContext Create(string dbName) =>
        new(new DbContextOptionsBuilder<ContactsDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options);
}
