using System;
using System.Globalization;
using System.Security.Principal;

namespace SnitzCore.Data.Extensions
{
    public static class SnitzDateExtensions
    {
        private const string DateFormat = "yyyyMMdd";
        private const string DateTimeFormat = "yyyyMMddHHmmss";
        
        /// <summary>
        /// Converts a forum date string to a <see cref="DateTime"/> object.
        /// </summary>
        /// <remarks>This method attempts to parse the input string using the current culture's settings.
        /// If the input string is 8 characters long, it is interpreted as a date in the "yyyyMMdd" format. If the input
        /// string is longer, the first 14 characters are interpreted as a date and time in the "yyyyMMddHHmmss"
        /// format.</remarks>
        /// <param name="date">The forum date string to convert. The string must be in one of the expected formats: "yyyyMMdd" (8
        /// characters) or "yyyyMMddHHmmss" (14 characters). If the string is null or does not match the expected
        /// formats, <see cref="DateTime.MinValue"/> is returned.</param>
        /// <returns>A <see cref="DateTime"/> object representing the parsed UTC date and time. If the input string is null, empty,
        /// or in an invalid format, <see cref="DateTime.MinValue"/> is returned.</returns>
        public static DateTime FromForumDateStr(this string? date)
        {
            if (date != null)
            {
                try
                {
                    if (date.Length == 8)
                    {
                        return DateTime.ParseExact(date, DateFormat, CultureInfo.CurrentCulture);
                    }

                    return DateTime.ParseExact(date.Substring(0, 14), DateTimeFormat, CultureInfo.CurrentCulture);
                }
                catch (Exception)
                {
                    return DateTime.MinValue;
                }

            }
            else
            {
                return DateTime.MinValue;
            }
        }
        public static DateTime FromForumDateStr(this string? date, bool dateonly)
        {
            if (!string.IsNullOrWhiteSpace(date))
            {
                if (date.Length == 8)
                {
                    return DateTime.ParseExact(date, DateFormat, CultureInfo.CurrentCulture);
                }
                return dateonly ? DateTime.ParseExact(date, DateTimeFormat, CultureInfo.CurrentCulture).Date : DateTime.ParseExact(date, DateTimeFormat, CultureInfo.CurrentCulture);
            }
            else
            {
                return dateonly ? DateTime.MinValue.Date : DateTime.MinValue;
            }
        }

        /// <summary>
        /// Converts the specified <see cref="DateTime"/> to a formatted string far saving to the database.
        /// </summary>
        /// <param name="date">The UTC <see cref="DateTime"/> to format.</param>
        /// <param name="dateonly">A value indicating whether to include only the date portion in the output.  If <see langword="true"/>, the
        /// output will include only the date; otherwise, both the date and time are included.</param>
        /// <returns>A string representation of the <paramref name="date"/> formatted according to the forum's date or date-time
        /// format.</returns>
        public static string ToForumDateStr(this DateTime date, bool dateonly = false)
        {
            if (dateonly)
            {
                return date.ToString(DateFormat);
            }
            return date.ToString(DateTimeFormat);
        }

        public static bool IsModerator(this IPrincipal user)
        {
            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                return false;
            }
            return user.IsInRole("Moderator");
        }
        /// <summary>
        /// Check if currentuser is a forum moderator
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool IsAdministrator(this IPrincipal user)
        {
            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                return false;
            }
            return user.IsInRole("Administrator");
        }  
    }
}
