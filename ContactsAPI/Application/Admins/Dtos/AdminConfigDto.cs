namespace ContactsAPI.Application.Admins.Dtos
{
    public class AdminConfigDto
    {
        //Being used by GetAdminConfigHandler
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;   // e.g. "MaxContactsPerUser"
        public string Value { get; set; } = string.Empty; // e.g. "100"
        public string Description { get; set; } = string.Empty;
    }
}
