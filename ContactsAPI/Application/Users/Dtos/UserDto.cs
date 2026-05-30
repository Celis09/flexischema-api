using ContactsAPI.Models;

namespace ContactsAPI.Application.Users.Dtos
{
    public class UserDto
    {
        public int UserId { get; set; }
        public int Sequence { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Editor";

        // Lifecycle fields
        public string Status { get; set; } = UserStatus.Active.ToString(); // 👈 enum type
        public DateTime CreatedDate { get; set; }
    }
}
