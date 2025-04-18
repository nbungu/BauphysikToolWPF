using System;

namespace BauphysikToolWPF.Services.Application
{
    public static class TimeStamp
    {
        public static long GetCurrentUnixTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public static string ConvertToNormalTime(long unixTimestamp)
        {
            // Convert Unix timestamp to DateTime
            DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime;

            // Format DateTime to "DD:MM:YYYY"
            string formattedDate = dateTime.ToString("dd.MM.yyyy");

            return formattedDate;
        }
        public static string ConvertToNormalTimeDetailed(long unixTimestamp)
        {
            // Convert Unix timestamp to DateTime
            DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime;

            // Format DateTime to "dd:MM:yyyy, HH:mm"
            string formattedDate = dateTime.ToString("dd.MM.yyyy HH:mm");

            return formattedDate;
        }
    }
}
