using ContactsAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Test.AdminTestHandler.Helpers;

public static class AdminDbFactory
{
    public static ContactsDbContext Create(string dbName) =>
        new(new DbContextOptionsBuilder<ContactsDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options);
}
