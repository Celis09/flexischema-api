using ContactsAPI.Application.Contacts.Dtos;
using MediatR;

namespace ContactsAPI.Application.Contacts.Queries.GetContactInsights
{
    public record GetContactInsightsQuery(int ContactId, bool ForceRegenerate = false) : IRequest<ContactInsightDto?>;
}
