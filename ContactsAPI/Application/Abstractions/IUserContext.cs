namespace ContactsAPI.Application.Abstractions
{
    public interface IUserContext
    {
        string UserId { get; }
        string Role { get; }
    }
}
