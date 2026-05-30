using ContactsAPI.Application.Abstractions;
using System.Security.Claims;

namespace ContactsAPI.Services
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string UserId =>
            _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
            ?? string.Empty;

        public string Role =>
    _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value
    ?? string.Empty;

    }
}


