﻿using Microsoft.Extensions.DependencyInjection;
using SnitzCore.Data.Interfaces;
using System;
using System.Globalization;

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
        public static string ToForumDisplay(this DateTime date, ISnitzConfig config)
        {
            var format = DateStr(config.GetValue("STRDATETYPE"));

            return date.ToLocalTime().ToString(format + " HH:mm", CultureInfo.CurrentCulture);
        }
        public static string DateStr(string dateformat)
        {
                switch (dateformat)
                {
                    case "mdy":
                        dateformat = "MM/dd/yy";
                        break;
                    case "dmy":
                        dateformat = "dd/MM/yy";
                        break;
                    case "ymd":
                        dateformat = "yy/MM/dd";
                        break;
                    case "ydm":
                        dateformat = "yy/dd/MM";
                        break;
                    case "dmmy":
                        dateformat = "dd MMM yyyy";
                        break;
                    case "mmdy":
                        dateformat = "MMM dd, yyyy";
                        break;
                    case "mmmdy":
                        dateformat = "MMMM dd, yyyy";
                        break;
                    case "dmmmy":
                        dateformat = "dd MMMM yyyy";
                        break;
                    default:
                        dateformat = "MMM dd, yyyy";
                        break;
            }
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
        public static string ToCustomDisplay(this DateTime date, string format)
        {
            return date.ToLocalTime().ToString(format, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Converts the specified <see cref="DateTime"/> to a string formatted for forum display.
        /// </summary>
        /// <param name="date">The <see cref="DateTime"/> to format.</param>
        /// <returns>A string representation of the <paramref name="date"/> in the format "dd/MM/yyyy HH:mm", using the current
        /// culture.</returns>
        public static string ToForumDateTimeDisplay(this DateTime date)
        {
            return date.ToLocalTime().ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture);
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
