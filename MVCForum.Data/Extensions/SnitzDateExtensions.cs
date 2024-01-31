using System;
using System.Globalization;

namespace SnitzCore.Data.Extensions
{
    public static class SnitzDateExtensions
    {
        public static DateTime FromForumDateStr(this string? date)
        {
            if (date != null)
            {
                if (date.Length == 8)
                {
                    return DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.CurrentCulture);
                }
                return DateTime.ParseExact(date, "yyyyMMddHHmmss", CultureInfo.CurrentCulture);
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
                    return DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.CurrentCulture);
                }
                return dateonly ? DateTime.ParseExact(date, "yyyyMMddHHmmss", CultureInfo.CurrentCulture).Date : DateTime.ParseExact(date, "yyyyMMddHHmmss", CultureInfo.CurrentCulture);
            }
            else
            {
                return dateonly ? DateTime.UtcNow.Date : DateTime.UtcNow;
            }
        }
        public static string ToForumDateStr(this DateTime date)
        {
            return date.ToString("yyyyMMddHHmmss");
        }
    }
}
