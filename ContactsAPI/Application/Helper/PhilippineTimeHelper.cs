namespace ContactsAPI.Application.Helper
{
    public static class PhilippineTime
    {
        private static readonly TimeZoneInfo PhTimeZone = GetPhilippineTimeZone();

        private static TimeZoneInfo GetPhilippineTimeZone()
        {
            // Try Linux/macOS first, fall back to Windows
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
            }
        }

        public static DateTime Now =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PhTimeZone);
    }
}
