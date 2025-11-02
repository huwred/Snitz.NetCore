using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Net;

namespace SnitzCore.Service.Extensions
{

    public static class DateTimeExtensions
    {
        /// <summary>
        /// Converts the specified <see cref="DateTime"/> to a string formatted for forum display.
        /// </summary>
        /// <param name="date">The <see cref="DateTime"/> to format.</param>
        /// <returns>A string representation of the <paramref name="date"/> in the format "MMM dd, yyyy HH:mm", using the current
        /// culture.</returns>
        public static string ToForumDisplay(this DateTime date, ISnitzConfig config, ISnitzCookie snitzCookie)
        {
            var format = DateStr(config.GetValue("STRDATETYPE"));

            return date.LocalTime(snitzCookie).ToString(format + ", HH:mm", CultureInfo.CurrentCulture);
        }
        /// <summary>
        /// Converts the specified UTC <see cref="DateTime"/> to the local time of the specified user based on their
        /// time zone settings.
        /// </summary>
        /// <remarks>This method retrieves the user's time zone information from their claims and caches
        /// it for performance optimization. If the time zone cannot be determined or an error occurs during the
        /// conversion, the method falls back to returning the input time as UTC.</remarks>
        /// <param name="date">The UTC <see cref="DateTime"/> to be converted.</param>
        /// <param name="manager">The <see cref="UserManager{TUser}"/> instance used to retrieve the user's claims.</param>
        /// <param name="user">The user whose time zone settings are used for the conversion.</param>
        /// <returns>A <see cref="DateTime"/> representing the input time converted to the user's local time. If the user's time
        /// zone is not set or cannot be determined, the input time is returned as UTC.</returns>
        public static DateTime LocalTime(this DateTime date,UserManager<ForumUser> manager, ForumUser user)
        {
            var userTimeZone = CacheProvider.GetOrCreate("TimeZone_" + user, () =>
            {
                var claims = manager.GetClaimsAsync(user).Result;
                var timeZoneClaim = claims.FirstOrDefault(c => c.Type == "TimeZone");
                if (timeZoneClaim != null)
                {
                    string timeZoneValue = timeZoneClaim.Value;
                    return TimeZoneInfo.FindSystemTimeZoneById(timeZoneValue);
                    // Use the value as needed
                } // Get user's time zone ID from claims
                return null;
            }, TimeSpan.FromMinutes(10));


            if (userTimeZone == null)
            {
                // Fallback to UTC if not set
                return DateTime.SpecifyKind(date, DateTimeKind.Utc);
            }
            try
            {
                
                //TimeZoneInfo userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(userTimeZoneId);
                DateTimeOffset userDateTimeOffset = TimeZoneInfo.ConvertTime(DateTime.SpecifyKind(date, DateTimeKind.Utc), userTimeZone);

                return userDateTimeOffset.DateTime;
            }
            catch (Exception)
            {
                // Fallback to UTC if not set
                return DateTime.SpecifyKind(date, DateTimeKind.Utc);
            }

        }
        /// <summary>
        /// Converts the specified UTC <see cref="DateTime"/> to the local time of the user based on their time zone
        /// settings.
        /// </summary>
        /// <remarks>This method retrieves the user's time zone from a cookie via the provided <paramref
        /// name="snitzCookie"/>. If the time zone is not set or invalid, the method falls back to returning the input
        /// <paramref name="date"/> as UTC. The user's time zone information is cached for 10 minutes to optimize
        /// performance.</remarks>
        /// <param name="date">The UTC <see cref="DateTime"/> to be converted.</param>
        /// <param name="snitzCookie">An implementation of <see cref="ISnitzCookie"/> used to retrieve the user's time zone information.</param>
        /// <returns>A <see cref="DateTime"/> representing the user's local time if their time zone is available; otherwise, the
        /// original <paramref name="date"/> adjusted to UTC.</returns>
        public static DateTime LocalTime(this DateTime date, ISnitzCookie snitzCookie)
        {
            var user = snitzCookie.CookieUser();
            var userTimeZone = CacheProvider.GetOrCreate("TimeZone_" + user, () =>
            {
                string? userTimeZoneId = snitzCookie.GetCookieValue("CookieTimeZone"); // Get user's time zone ID from cookie
                if (!string.IsNullOrEmpty(userTimeZoneId))
                {
                    try
                    {
                        return TimeZoneInfo.FindSystemTimeZoneById(userTimeZoneId);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
                return null;
            }, TimeSpan.FromMinutes(10));


            if (userTimeZone == null)
            {
                // Fallback to UTC if not set
                return DateTime.SpecifyKind(date, DateTimeKind.Utc);
            }
            try
            {
                
                //TimeZoneInfo userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(userTimeZoneId);
                DateTimeOffset userDateTimeOffset = TimeZoneInfo.ConvertTime(DateTime.SpecifyKind(date, DateTimeKind.Utc), userTimeZone);

                return userDateTimeOffset.DateTime;
            }
            catch (Exception)
            {
                // Fallback to UTC if not set
                return DateTime.SpecifyKind(date, DateTimeKind.Utc);
            }

        }
        public static string DateStr(string dateformat)
        {
            dateformat = dateformat switch
            {
                "mdy" => "MM/dd/yy",
                "dmy" => "dd/MM/yy",
                "ymd" => "yy/MM/dd",
                "ydm" => "yy/dd/MM",
                "dmmy" => "dd MMM yyyy",
                "mmdy" => "MMM dd, yyyy",
                "mmmdy" => "MMMM dd, yyyy",
                "dmmmy" => "dd MMMM yyyy",
                _ => "MMM dd, yyyy",
            };
            return dateformat;
        }
        /// <summary>
        /// Converts the specified <see cref="DateTime"/> to its string representation using the specified format and
        /// the current culture.
        /// </summary>
        /// <remarks>This method uses the current culture to format the <see cref="DateTime"/>. To use a
        /// specific culture, consider using <see cref="DateTime.ToString(string, IFormatProvider)"/>
        /// directly.</remarks>
        /// <param name="date">The <see cref="DateTime"/> to format.</param>
        /// <param name="format">A standard or custom date and time format string. See <see cref="DateTime.ToString(string)"/> for valid
        /// format specifiers.</param>
        /// <returns>A string representation of the <paramref name="date"/> formatted according to <paramref name="format"/> and
        /// the current culture.</returns>
        public static string ToCustomDisplay(this DateTime date, string format, ISnitzCookie snitzCookie)
        {
            return date.LocalTime(snitzCookie).ToString(format, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Converts the specified <see cref="DateTime"/> to a string formatted for forum display.
        /// </summary>
        /// <param name="date">The <see cref="DateTime"/> to format.</param>
        /// <returns>A string representation of the <paramref name="date"/> in the format "dd/MM/yyyy HH:mm", using the current
        /// culture.</returns>
        public static string ToForumDateTimeDisplay(this DateTime date, ISnitzCookie snitzCookie)
        {
            return date.LocalTime(snitzCookie).ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Converts the specified <see cref="DateTime"/> to a string formatted as "dd/MM/yyyy" using the current
        /// culture.
        /// </summary>
        /// <param name="date">The <see cref="DateTime"/> to format.</param>
        /// <returns>A string representation of the date in "dd/MM/yyyy" format.</returns>
        public static string ToForumDateDisplay(this DateTime date)
        {

            return date.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Converts the specified <see cref="DateTime"/> to a string representation in ISO 8601 format.
        /// </summary>
        /// <param name="date">The <see cref="DateTime"/> to convert. If the value is <see cref="DateTime.MinValue"/>, an empty string is
        /// returned.</param>
        /// <returns>A string representing the <paramref name="date"/> in the format "yyyy-MM-ddTHH:mm:ssZ", or an empty string
        /// if the input is <see cref="DateTime.MinValue"/>.</returns>
        public static string ToTimeagoDate(this DateTime date)
        {
            if (date == DateTime.MinValue)
            {
                return String.Empty;
            }
            return date.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

    }


}
