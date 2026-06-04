using ContactsAPI.Application.Contacts.Queries.SearchContactsByAi;
using MediatR;

namespace ContactsAPI.Application.Contacts.Queries.SearchContactsByAi
{
    public record SearchContactsByAiQuery(string SearchPrompt) : IRequest<SearchContactsByAiQueryResult>;
}
