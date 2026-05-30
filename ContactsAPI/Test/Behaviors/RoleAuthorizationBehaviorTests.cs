using ContactsAPI.Application.Abstractions;
using ContactsAPI.Application.Attributes;
using ContactsAPI.Application.Behaviors;
using ContactsAPI.Application.Exceptions;
using MediatR;
using Xunit;

namespace ContactsAPI.Test.Behaviors
{
    public class RoleAuthorizationBehaviorTests
    {
        private class FakeUserContext : IUserContext
        {
            public string UserId => "123";
            public string Role { get; set; } = "Viewer";
        }

        [AuthorizeRole("Admin")]
        private class AdminOnlyCommand : IRequest<bool> { }

        [Fact]
        public async Task Handle_ShouldThrow_WhenRoleNotAuthorized()
        {
            var userContext = new FakeUserContext { Role = "Viewer" };
            var behavior = new RoleAuthorizationBehavior<AdminOnlyCommand, bool>(userContext);

            var request = new AdminOnlyCommand();

            // ✅ FIX: behavior throws UnauthorizedAccessAppException, not the BCL UnauthorizedAccessException
            await Assert.ThrowsAsync<UnauthorizedAccessAppException>(() =>
                behavior.Handle(request, () => Task.FromResult(true), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldPass_WhenRoleAuthorized()
        {
            var userContext = new FakeUserContext { Role = "Admin" };
            var behavior = new RoleAuthorizationBehavior<AdminOnlyCommand, bool>(userContext);

            var request = new AdminOnlyCommand();

            var result = await behavior.Handle(request, () => Task.FromResult(true), CancellationToken.None);

            Assert.True(result);
        }
    }
}