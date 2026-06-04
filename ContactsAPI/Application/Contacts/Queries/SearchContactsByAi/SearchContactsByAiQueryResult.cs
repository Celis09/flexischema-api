using ContactsAPI.Application.Contacts.Dtos;
using System.Collections.Generic;

namespace ContactsAPI.Application.Contacts.Queries.SearchContactsByAi
{
    public class SearchContactsByAiQueryResult
    {
        public List<ContactDto> Contacts { get; set; } = new();
        public bool IsAiFallback { get; set; }
    }
}
