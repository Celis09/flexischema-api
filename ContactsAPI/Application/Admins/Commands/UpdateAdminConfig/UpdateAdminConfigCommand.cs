using MediatR;

namespace ContactsAPI.Application.Admins.Commands.UpdateAdminConfig
{
    public class UpdateAdminConfigCommand : IRequest<int>
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
