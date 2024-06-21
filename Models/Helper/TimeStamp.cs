﻿using System;

namespace BauphysikToolWPF.Models.Helper
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
    }
}