using ContactsAPI.Application.Users.Queries.GetAllUsers;
using ContactsAPI.Data;
using ContactsAPI.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ContactsAPI.Test.UserHandlerTest;

public class GetUsersHandlerTests
{
    private ContactsDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ContactsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // unique per test
            .Options;
        return new ContactsDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllUsers()
    {
        var context = GetDbContext();
        context.Users.AddRange(
            new User { Username = "u1", Email = "u1@example.com", Role = "Viewer" },
            new User { Username = "u2", Email = "u2@example.com", Role = "Admin" }
        );
        await context.SaveChangesAsync();

        var handler = new GetAllUsersHandler(context);
        var query = new GetAllUsersQuery { Page = 1, PageSize = 10 };

        var result = await handler.Handle(query, default);

        Assert.Equal(2, result.TotalCount);              // check total count
        Assert.Equal(2, result.Items.Count);             // check items count
        Assert.Contains(result.Items, u => u.Role == "Viewer");
        Assert.Contains(result.Items, u => u.Role == "Admin");
    }

    [Fact]
    public async Task Handle_ShouldFilterByRole()
    {
        var context = GetDbContext();
        context.Users.AddRange(
            new User { Username = "u1", Email = "u1@example.com", Role = "Viewer" },
            new User { Username = "u2", Email = "u2@example.com", Role = "Admin" }
        );
        await context.SaveChangesAsync();

        var handler = new GetAllUsersHandler(context);
        var query = new GetAllUsersQuery { Search = "Admin", Page = 1, PageSize = 10 };

        var result = await handler.Handle(query, default);

        Assert.Single(result.Items);                     // only one user returned
        Assert.Equal("Admin", result.Items.First().Role);
    }
}

