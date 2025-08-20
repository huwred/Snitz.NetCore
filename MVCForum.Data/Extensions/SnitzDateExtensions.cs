using System;
using System.Globalization;
using System.Security.Principal;

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
