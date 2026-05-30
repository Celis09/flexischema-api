using ContactsAPI.Application.Exceptions;
using ContactsAPI.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.Application.Admins.Commands.UpdateAdminConfig
{
    public class UpdateAdminConfigHandler(ContactsDbContext context) : IRequestHandler<UpdateAdminConfigCommand, int>
    {
        public async Task<int> Handle(UpdateAdminConfigCommand request, CancellationToken ct)
        {
            var config = await context.AdminConfigs
                .FindAsync([request.Id], ct)
                ?? throw new NotFoundException($"Config {request.Id} not found");

            if (request.Key == AdminConfigConstants.LimitConfigKey
                && int.TryParse(request.Value, out var newMax))
            {
                var activeCount = await context.ExtraFieldDefinitions
                    .CountAsync(f => f.IsActive, ct);

                if (activeCount > newMax)
                    throw new ContactsAPI.Application.Exceptions.ValidationException(
                        new Dictionary<string, string[]>
                        {
                            {
                                "Value", new[]
                                {
                                    $"Cannot set {AdminConfigConstants.LimitConfigKey} to {newMax} — " +
                                    $"{activeCount} fields are currently active. " +
                                    $"Disable at least {activeCount - newMax} before lowering the limit."
                                }
                            }
                        });
            }

            config.Value = request.Value;
            await context.SaveChangesAsync(ct);
            return config.Id;
        }
    }
}