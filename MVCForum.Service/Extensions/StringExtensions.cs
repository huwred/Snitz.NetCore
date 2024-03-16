using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnitzCore.Service.Extensions
{
    public static class StringExtensions
    {
        public static string ToEnglishNumber(this string str)
        {
            string englishNumbers = "";

            for (int i = 0; i < str.Length; i++)
            {
                if (Char.IsDigit(str[i]))
                {
                    englishNumbers += char.GetNumericValue(str, i);
                }
                else
                {
                    englishNumbers += str[i].ToString();
                }
            }
            return englishNumbers;
        }
    }
}
