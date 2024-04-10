using System;
using System.Globalization;

namespace SnitzCore.Service.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToForumDisplay(this DateTime date)
        {
            return date.ToString("MMM dd, yyyy HH:mm", CultureInfo.CurrentCulture);
        }
        public static string ToForumDateTimeDisplay(this DateTime date)
        {
            return date.ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture);
        }
        public static string ToForumDateDisplay(this DateTime date)
        {

            return date.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture);
        }
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
