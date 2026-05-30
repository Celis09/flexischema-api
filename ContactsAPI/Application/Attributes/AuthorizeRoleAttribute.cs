namespace ContactsAPI.Application.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AuthorizeRoleAttribute(params string[] roles) : Attribute
    {
        public string[] Roles { get; } = roles;
    }
}