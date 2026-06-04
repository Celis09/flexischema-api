using System;
using System.Collections.Generic;

namespace ContactsAPI.Application.Contacts.Dtos
{
    public class ContactFilterDto
    {
        public string? Status { get; set; }
        public string? SearchTerm { get; set; }
        public DateTime? AddedAfter { get; set; }
        public Dictionary<string, string>? ExtraFieldFilters { get; set; }
    }
}
