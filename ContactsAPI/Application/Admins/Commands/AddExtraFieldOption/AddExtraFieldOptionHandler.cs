using ContactsAPI.Data;
using ContactsAPI.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ContactsAPI.Application.Admins.Commands.AddExtraFieldOption;

public class AddExtraFieldOptionHandler(ContactsDbContext context, IMemoryCache cache)
    : IRequestHandler<AddExtraFieldOptionCommand, AddExtraFieldOptionResult>
{
    public async Task<AddExtraFieldOptionResult> Handle(
        AddExtraFieldOptionCommand request,
        CancellationToken cancellationToken)
    {
        var maxOrder = await context.ExtraFieldOptions
            .Where(o => o.ExtraFieldDefinitionId == request.DefinitionId)
            .Select(o => (int?)o.DisplayOrder)
            .MaxAsync(cancellationToken) ?? -1;

        var option = new ExtraFieldOption
        {
            ExtraFieldDefinitionId = request.DefinitionId,
            OptionValue = request.OptionValue.Trim(),
            DisplayOrder = maxOrder + 1
        };

        context.ExtraFieldOptions.Add(option);
        await context.SaveChangesAsync(cancellationToken);
        cache.Remove("extrafield:definitions:withOptions");

        return new AddExtraFieldOptionResult(
            option.ExtraFieldOptionId,
            option.OptionValue,
            option.DisplayOrder);
    }
}