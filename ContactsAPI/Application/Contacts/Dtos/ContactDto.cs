using ContactsAPI.Entities;

namespace ContactsAPI.Application.Contacts.Dtos
{
    public class PublicContactDto
    {
        public int Sequence { get; set; }
        public string Name { get; set; } = "";
        public string? Email { get; set; }
        public List<ContactExtraFieldResponse> ExtraFields { get; set; } = [];
    }

    public class EditorContactDto : PublicContactDto
    {
        public int Id { get; set; }
    }

    public class ContactDto : PublicContactDto
    {
        public int Id { get; set; } // internal/admin only
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; } = ContactStatus.Active.ToString();
    }
}
