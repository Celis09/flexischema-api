using MediatR;

namespace ContactsAPI.Application.Admins.Commands.AddExtraFieldOption;

public record AddExtraFieldOptionCommand(
    int DefinitionId,
    string OptionValue
) : IRequest<AddExtraFieldOptionResult>;

public record AddExtraFieldOptionResult(
    int ExtraFieldOptionId,
    string OptionValue,
    int DisplayOrder
);