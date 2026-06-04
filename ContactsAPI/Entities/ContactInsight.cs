namespace ContactsAPI.Entities
{
    public class ContactInsight
    {
        public int ContactId { get; set; }
        public string Summary { get; set; } = "";
        public string Tag { get; set; } = "Lead";
        public DateTime GeneratedAt { get; set; }
        public Contact? Contact { get; set; }
    }
}
