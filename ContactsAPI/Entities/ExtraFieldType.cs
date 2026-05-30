namespace ContactsAPI.Models
{
    public enum ExtraFieldType
    {
        Text,
        Email,
        Option,
        Number,
        Phone,
        Date,
        Url
    }

    public static class ExtraFieldTypeExtensions
    {
        public static string ToLabel(this ExtraFieldType fieldType) => fieldType switch
        {
            ExtraFieldType.Text => "text",
            ExtraFieldType.Email => "email",
            ExtraFieldType.Option => "option (select one)",
            ExtraFieldType.Number => "number",
            ExtraFieldType.Phone => "phone",
            ExtraFieldType.Date => "date (YYYY-MM-DD)",
            ExtraFieldType.Url => "url",
            _ => "text"
        };
    }
}