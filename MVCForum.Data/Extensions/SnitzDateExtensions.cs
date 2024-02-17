using System;
using System.Globalization;

namespace SnitzCore.Data.Extensions
{
    public static class SnitzDateExtensions
    {
        private const string DateFormat = "yyyyMMdd";
        private const string DateTimeFormat = "yyyyMMddHHmmss";
        public static DateTime FromForumDateStr(this string? date)
        {
            if (date != null)
            {
                if (date.Length == 8)
                {
                    return DateTime.ParseExact(date, DateFormat, CultureInfo.CurrentCulture);
                }
                return DateTime.ParseExact(date.Substring(0, 14), DateTimeFormat, CultureInfo.CurrentCulture);
            }
            else
            {
                return DateTime.UtcNow;
            }
        }
        public static DateTime FromForumDateStr(this string? date, bool dateonly)
        {
            if (date != null)
            {
                if (date.Length == 8)
                {
                    return DateTime.ParseExact(date, DateFormat, CultureInfo.CurrentCulture);
                }
                return dateonly ? DateTime.ParseExact(date, DateTimeFormat, CultureInfo.CurrentCulture).Date : DateTime.ParseExact(date, DateTimeFormat, CultureInfo.CurrentCulture);
            }
            else
            {
                return dateonly ? DateTime.UtcNow.Date : DateTime.UtcNow;
            }
        }
        public static string ToForumDateStr(this DateTime date)
        {
            return date.ToString(DateTimeFormat);
        }
    }
}
