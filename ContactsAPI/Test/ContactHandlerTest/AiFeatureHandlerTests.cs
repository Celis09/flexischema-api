using ContactsAPI.Application.Abstractions;
using ContactsAPI.Application.Contacts.Commands.MapCsvHeaders;
using ContactsAPI.Application.Contacts.Dtos;
using ContactsAPI.Application.Contacts.Queries.GetContactInsights;
using ContactsAPI.Application.Contacts.Queries.SearchContactsByAi;
using ContactsAPI.Entities;
using ContactsAPI.Models;
using ContactsAPI.Test.ContactHandlerTest.Helpers;
using Microsoft.Extensions.AI;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ContactsAPI.Test.ContactHandlerTest
{
    public class AiFeatureHandlerTests
    {
        [Fact]
        public async Task GetContactInsights_ReturnsInsights_Correctly()
        {
            // Arrange
            await using var context = DbFactory.Create(nameof(GetContactInsights_ReturnsInsights_Correctly));
            var contact = new Contact { Name = "Jane Doe", Email = "jane@example.com", Status = ContactStatus.Active };
            context.Contacts.Add(contact);
            await context.SaveChangesAsync();

            var chatClientMock = new Mock<IChatClient>();
            var rawJsonResult = "{\"Summary\": \"Jane Doe is an active client. She has been communicating regularly.\", \"Tag\": \"Active\"}";
            var mockResponse = new ChatResponse(new ChatMessage(ChatRole.Assistant, rawJsonResult));

            chatClientMock.Setup(c => c.GetResponseAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse);

            var handler = new GetContactInsightsQueryHandler(context, chatClientMock.Object);

            // Act
            var result = await handler.Handle(new GetContactInsightsQuery(contact.Id), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Jane Doe is an active client. She has been communicating regularly.", result.Summary);
            Assert.Equal("Active", result.Tag);
        }

        [Fact]
        public async Task MapCsvHeaders_ReturnsMappings_Correctly()
        {
            // Arrange
            await using var context = DbFactory.Create(nameof(MapCsvHeaders_ReturnsMappings_Correctly));
            var chatClientMock = new Mock<IChatClient>();
            var rawJsonResult = "{\"First Name\": \"Name\", \"Mail\": \"Email\", \"Phone Number\": \"\"}";
            var mockResponse = new ChatResponse(new ChatMessage(ChatRole.Assistant, rawJsonResult));

            chatClientMock.Setup(c => c.GetResponseAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse);

            var handler = new MapCsvHeadersCommandHandler(context, chatClientMock.Object);
            var command = new MapCsvHeadersCommand
            {
                CsvHeaders = new List<string> { "First Name", "Mail", "Phone Number" },
                SampleData = new List<List<string>> { new List<string> { "John", "john@example.com", "12345" } }
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Name", result["First Name"]);
            Assert.Equal("Email", result["Mail"]);
            Assert.Equal("", result["Phone Number"]);
        }

        [Fact]
        public async Task SearchContactsByAi_AdminUser_RespectsAiFilter()
        {
            // Arrange
            await using var context = DbFactory.Create(nameof(SearchContactsByAi_AdminUser_RespectsAiFilter));
            var activeContact = new Contact { Name = "Active Guy", Status = ContactStatus.Active };
            var inactiveContact = new Contact { Name = "Inactive Guy", Status = ContactStatus.Inactive };
            context.Contacts.AddRange(activeContact, inactiveContact);
            await context.SaveChangesAsync();

            var chatClientMock = new Mock<IChatClient>();
            var rawJsonResult = "{\"Status\": \"Inactive\", \"SearchTerm\": \"Guy\", \"HasPhone\": null, \"AddedAfter\": null}";
            var mockResponse = new ChatResponse(new ChatMessage(ChatRole.Assistant, rawJsonResult));

            chatClientMock.Setup(c => c.GetResponseAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse);

            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(u => u.Role).Returns("Admin");

            var handler = new SearchContactsByAiQueryHandler(context, chatClientMock.Object, userContextMock.Object);

            // Act
            var result = await handler.Handle(new SearchContactsByAiQuery("Show inactive guys"), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsAiFallback);
            Assert.Single(result.Contacts);
            Assert.Equal("Inactive Guy", result.Contacts.First().Name);
        }

        [Fact]
        public async Task SearchContactsByAi_EditorUser_OverridesAiStatus_EnforcesActive()
        {
            // Arrange
            await using var context = DbFactory.Create(nameof(SearchContactsByAi_EditorUser_OverridesAiStatus_EnforcesActive));
            var activeContact = new Contact { Name = "Active Guy", Status = ContactStatus.Active };
            var inactiveContact = new Contact { Name = "Inactive Guy", Status = ContactStatus.Inactive };
            context.Contacts.AddRange(activeContact, inactiveContact);
            await context.SaveChangesAsync();

            var chatClientMock = new Mock<IChatClient>();
            var rawJsonResult = "{\"Status\": \"Inactive\", \"SearchTerm\": \"Guy\", \"HasPhone\": null, \"AddedAfter\": null}";
            var mockResponse = new ChatResponse(new ChatMessage(ChatRole.Assistant, rawJsonResult));

            chatClientMock.Setup(c => c.GetResponseAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse);

            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(u => u.Role).Returns("Editor"); // Editor role

            var handler = new SearchContactsByAiQueryHandler(context, chatClientMock.Object, userContextMock.Object);

            // Act
            var result = await handler.Handle(new SearchContactsByAiQuery("Show inactive guys"), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsAiFallback);
            // Must return active contact, completely ignoring the "Inactive" request from AI status filter because of guardrails
            Assert.Single(result.Contacts);
            Assert.Equal("Active Guy", result.Contacts.First().Name);
        }

        [Fact]
        public async Task SearchContactsByAi_WhenAiFails_GracefullyFallsBackToStandardSearch()
        {
            // Arrange
            await using var context = DbFactory.Create(nameof(SearchContactsByAi_WhenAiFails_GracefullyFallsBackToStandardSearch));
            var activeContact = new Contact { Name = "Unique Name Active", Status = ContactStatus.Active };
            context.Contacts.Add(activeContact);
            await context.SaveChangesAsync();

            var chatClientMock = new Mock<IChatClient>();
            chatClientMock.Setup(c => c.GetResponseAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("API Offline"));

            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(u => u.Role).Returns("Viewer");

            var handler = new SearchContactsByAiQueryHandler(context, chatClientMock.Object, userContextMock.Object);

            // Act
            var result = await handler.Handle(new SearchContactsByAiQuery("Unique Name Active"), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsAiFallback); // Fallback flag must be true
            Assert.Single(result.Contacts);
            Assert.Equal("Unique Name Active", result.Contacts.First().Name);
        }
    }
}
