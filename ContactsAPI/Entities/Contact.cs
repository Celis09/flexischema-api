using ContactsAPI.Application.Helper;

namespace ContactsAPI.Entities
{
    public enum ContactStatus
    {
        Active,
        Inactive,
        Archived
    }
    public class Contact
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Email { get; set; }

        public ContactStatus Status { get; set; } = ContactStatus.Active;
        public DateTime CreatedDate { get; set; } = PhilippineTime.Now;

        public ICollection<ContactExtraField> ExtraFields { get; set; } = new List<ContactExtraField>();
    }
}
