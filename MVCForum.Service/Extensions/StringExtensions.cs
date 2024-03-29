﻿using System;
using System.Collections.Generic;
using System.Globalization;
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
        public static string ToLangNum(this long i, string lang)
        {
            if (lang == "fa")
            {
                CultureInfo ci = new CultureInfo(lang);
                return i.ConvertDigitChar(ci);
            }
            return i.ToString();
        }
        public static string ToLangNum(this int i, string lang)
        {
            if (lang == "fa")
            {
                CultureInfo ci = new CultureInfo(lang);
                return i.ConvertDigitChar(ci);
            }
            return i.ToString();
        }

        private static string ConvertDigitChar(this int digit, CultureInfo destination)
        {
            string res = digit.ToString();
            for (int i = 0; i <= 9; i++)
            {
                res = res.Replace(i.ToString(), destination.NumberFormat.NativeDigits[i]);
            }
            return res;
        }
        private static string ConvertDigitChar(this long digit, CultureInfo destination)
        {
            string res = digit.ToString();
            for (int i = 0; i <= 9; i++)
            {
                res = res.Replace(i.ToString(), destination.NumberFormat.NativeDigits[i]);
            }
            return res;
        }
    }
}
