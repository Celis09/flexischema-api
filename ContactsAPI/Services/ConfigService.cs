using ContactsAPI.Application.Abstractions;
using ContactsAPI.Application.Admins.Dtos;
using ContactsAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ContactsAPI.Services
{
    // FIX #3: added IMemoryCache so AdminConfig values are not re-queried on
    // every contact create / update. GetIntAsync("MaxExtraFieldsPerContact") was
    // hitting the DB on every single write request. Config values rarely change,
    // so a 5-minute in-process cache is safe and eliminates the redundant queries.
    //
    // When an admin updates a config via UpdateAdminConfigHandler, call
    //   cache.Remove("config:<key>")
    // in that handler to invalidate the cached entry immediately.
    public class ConfigService : IConfigService
    {
        private readonly ContactsDbContext _context;
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

        public ConfigService(ContactsDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<string?> GetValueAsync(string key, CancellationToken ct = default)
            => await _cache.GetOrCreateAsync(
                $"config:{key}",
                async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = CacheTtl;
                    return await _context.AdminConfigs
                        .Where(c => c.Key == key)
                        .Select(c => c.Value)
                        .FirstOrDefaultAsync(ct);
                });

        public async Task<bool> GetBoolAsync(string key, CancellationToken ct = default)
            => bool.TryParse(await GetValueAsync(key, ct), out var result) && result;

        public async Task<int> GetIntAsync(string key, CancellationToken ct = default)
            => int.TryParse(await GetValueAsync(key, ct), out var result) ? result : 0;

        public async Task<IEnumerable<ExtraFieldDefinitionDto>> GetExtraFieldDefinitionsAsync(
            CancellationToken ct = default)
            => await _context.ExtraFieldDefinitions
                .AsNoTracking()
                .Select(d => new ExtraFieldDefinitionDto
                {
                    ExtraFieldDefinitionId = d.ExtraFieldDefinitionId,
                    FieldName = d.FieldName
                })
                .ToListAsync(ct);
    }
}