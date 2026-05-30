using ContactsAPI.Application.Users.Commands.CreateUser;
using ContactsAPI.Application.Users.Commands.UpdateUser;
using ContactsAPI.Application.Users.Dtos;
using ContactsAPI.Data;
using ContactsAPI.Test.IntegrationTest.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Xunit;

namespace ContactsAPI.Test.IntegrationTest;

public class UsersControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _dbName = Guid.NewGuid().ToString();

    public UsersControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var dbDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ContactsDbContext>));
                if (dbDescriptor != null)
                    services.Remove(dbDescriptor);

                services.AddDbContext<ContactsDbContext>(opts =>
                    opts.UseInMemoryDatabase(_dbName));

                services.AddAuthentication("FakeScheme")
                    .AddScheme<AuthenticationSchemeOptions, FakeAuthHandler>(
                        "FakeScheme", _ => { });
            });
        });
    }

    [Fact]
    public async Task Post_CreateUser_ShouldReturnCreated()
    {
        var client = _factory.CreateClient();
        var command = new CreateUserCommand
        {
            Username = "integrationUser",
            Email = "int@example.com",
            Password = "Secret123!",
            Role = "Editor"
        };

        var response = await client.PostAsJsonAsync("/api/v1/admin/users", command);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Get_GetUserById_ShouldReturnUser()
    {
        var client = _factory.CreateClient();
        var command = new CreateUserCommand
        {
            Username = "getUser",
            Email = "get@example.com",
            Password = "Secret123!",
            Role = "Editor"
        };

        var createResponse = await client.PostAsJsonAsync("/api/v1/admin/users", command);
        var id = await createResponse.Content.ReadFromJsonAsync<int>();

        var response = await client.GetAsync($"/api/v1/admin/users/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user!.Username.Should().Be("getUser");
    }

    [Fact]
    public async Task Put_UpdateUser_ShouldReturnNoContent()
    {
        var client = _factory.CreateClient();
        var command = new CreateUserCommand
        {
            Username = "updateMe",
            Email = "update@example.com",
            Password = "Secret123!",
            Role = "Editor"
        };

        await client.PostAsJsonAsync("/api/v1/admin/users", new CreateUserCommand { Username = "u1", Email = "e1@x.com", Password = "Password@123", Role = "Editor" });
        await client.PostAsJsonAsync("/api/v1/admin/users", new CreateUserCommand { Username = "u2", Email = "e2@x.com", Password = "Password@123", Role = "Editor" });
        var createResponse = await client.PostAsJsonAsync("/api/v1/admin/users", command);
        
        var id = await createResponse.Content.ReadFromJsonAsync<int>();

        var update = new UpdateUserCommand
        {
            UserId = id,
            Username = "updatedName",
            Email = "updated@example.com",
            Role = "Admin"
        };

        var response = await client.PutAsJsonAsync($"/api/v1/admin/users/{id}", update);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}