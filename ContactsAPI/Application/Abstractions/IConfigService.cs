
using ContactsAPI.Application.Admins.Dtos;

namespace ContactsAPI.Application.Abstractions
{
    public interface IConfigService
    {
        Task<string?> GetValueAsync(string key, CancellationToken ct = default);
        Task<bool> GetBoolAsync(string key, CancellationToken ct = default);
        Task<int> GetIntAsync(string key, CancellationToken ct = default);
        Task<IEnumerable<ExtraFieldDefinitionDto>> GetExtraFieldDefinitionsAsync(CancellationToken ct = default);
    }
}