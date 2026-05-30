using ContactsAPI.Application.Admins.Dtos;
using MediatR;

namespace ContactsAPI.Application.Admins.Queries.GetExtraFieldDefinitions
{
    public class GetExtraFieldDefinitionsQuery : IRequest<List<ExtraFieldDefinitionDto>>
    {
        // Optional filters: role, active status
        public string? RoleFilter { get; set; }
        public bool? IsActive { get; set; }
    }
}
