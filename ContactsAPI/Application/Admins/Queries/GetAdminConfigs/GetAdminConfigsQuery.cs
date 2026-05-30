using ContactsAPI.Application.Admins.Dtos;
using MediatR;

namespace ContactsAPI.Application.Admins.Queries.GetAdminConfigs
{
    public class GetAdminConfigsQuery : IRequest<List<AdminConfigDto>>
    {
    }
}
