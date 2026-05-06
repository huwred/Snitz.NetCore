using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace SnitzCore.Service.Extensions
{
    public static class StringExtensions
    {
        public static string UrlCombine(string uri1, string uri2)
        {
            uri1 = uri1.TrimEnd('/');
            uri2 = uri2.TrimStart('/');
            return $"{uri1}/{uri2}";
        }
        
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
        
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
        
        public static string GetReverseDNS(string ip, int timeout)
        {
            try
            {
                GetHostEntryHandler callback = new GetHostEntryHandler(Dns.GetHostEntry);
                IAsyncResult result = callback.BeginInvoke(ip, null, null);
                if (result.AsyncWaitHandle.WaitOne(timeout * 1000, false))
                {
                    return callback.EndInvoke(result).HostName;
                }
                return ip;
            }
            catch (Exception)
            {
                return ip;
            }
        }
        
        public static string LocalReturnUrl(this string? returnUrl,string? forumurl)
        {
            if(returnUrl != null && forumurl != null)
            {
                return returnUrl.Replace(forumurl,"");
            }
            return string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl;
        }
        
        /// <summary>
        /// Turns a List<typeparamref name="T"/> into a csv file
        /// </summary>
        /// <typeparam name="T">Type of Object</typeparam>
        /// <param name="list">List of objects to convert</param>
        /// <param name="path">Optional file path to write CSV to disk</param>
        /// <param name="include">Comma separated string of properties to include</param>
        /// <param name="exclude">Comma separated string of properties to exclude</param>
        /// <remarks>
        /// <paramref name="include"/> + <paramref name="exclude"/> are mutually exclusive
        /// If <paramref name="include"/> is non empty, only those properties defined will be exported
        /// OR If <paramref name="exclude"/> is non empty, all properties except those defined will be included
        /// </remarks>
        /// <returns>CSV formatted string</returns>
        public static string? ToCSV<T>(this IList<T> list, string path = "", string include = "", string exclude = "")
        {
            return CreateCsvFile(list, path, include, exclude);
        }

        public static string TruncateLongString(this string str, int maxLength)
        {
            if (String.IsNullOrWhiteSpace(str))
                return str;
            return str.Substring(0, Math.Min(str.Length, maxLength));
        }

        private static string? CreateCsvFile<T>(IList<T> list, string path, string include, string exclude)
        {
            StringBuilder sb = new StringBuilder();

            PropertyInfo[] props = typeof(T).GetProperties();
            List<PropertyInfo> propList = GetSelectedProperties(props, include, exclude);

            foreach (var item in list)
            {
                List<string?> propValues = new List<string?>();

                foreach (var prop in propList)
                {
                    var propval = prop.GetValue(item, null);
                    if (propval == null)
                    {
                        propValues.Add(null);
                    }
                    else
                    {
                        // FIX: Type-safe formatting based on actual property type
                        if (prop.PropertyType == typeof(string))
                        {
                            // Escape quotes and wrap in quotes for CSV
                            propValues.Add(string.Format("\"{0}\"", ((string)propval).Replace("\"", "'")));
                        }
                        else
                        {
                            // Non-string types don't need quote wrapping
                            propValues.Add(propval.ToString());
                        }
                    }
                }

                string line = string.Join(",", propValues);
                sb.AppendLine(line);
            }
            
            if (!string.IsNullOrEmpty(sb.ToString()))
            {
                // FIX: Implement file writing if path is provided
                if (!string.IsNullOrEmpty(path))
                {
                    File.WriteAllText(path, sb.ToString());
                }
                return sb.ToString();
            }
            return null;
        }
        
        private static List<PropertyInfo> GetSelectedProperties(PropertyInfo[] props, string include, string exclude)
        {
            List<PropertyInfo> propList = new List<PropertyInfo>();
            
            // FIX: Consistent string empty checks
            if (!string.IsNullOrEmpty(include))
            {
                var includeProps = include.ToLower().Split(',').ToList();
                foreach (var item in props)
                {
                    var propName = includeProps.FirstOrDefault(a => a == item.Name.ToLower());
                    if (!string.IsNullOrEmpty(propName))
                        propList.Add(item);
                }
            }
            else if (!string.IsNullOrEmpty(exclude))
            {
                var excludeProps = exclude.ToLower().Split(',');
                foreach (var item in props)
                {
                    var propName = excludeProps.FirstOrDefault(a => a == item.Name.ToLower());
                    if (string.IsNullOrEmpty(propName))
                        propList.Add(item);
                }
            }
            else
            {
                propList.AddRange(props.ToList());
            }
            return propList;
        }
        
        private delegate IPHostEntry GetHostEntryHandler(string ip);
        
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
