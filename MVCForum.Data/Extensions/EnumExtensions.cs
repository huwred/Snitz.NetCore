using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace SnitzCore.Data.Extensions
{
    public static class EnumExtensions
    {
        public static IEnumerable<SelectListItem> GetEnumSelectList(this Type enumtype)
        {
            return (Enum.GetValues(enumtype).Cast<int>().Select(e =>
                new SelectListItem() { Text = GetDisplayName(enumtype,Enum.GetName(enumtype, e)!), Value = e.ToString() })).ToList();
        }
        public static Type? GetEnumType(string name)
        {
            return 
                (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                    let type = assembly.GetType(name)
                    where type is { IsEnum: true }
                    select type).FirstOrDefault();

        }
        public static TAttribute GetAttribute<TAttribute>(this Enum enumValue) 
            where TAttribute : Attribute
        {
            var test = enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First().GetCustomAttribute<TAttribute>();
            return test;
        }

        public static string? GetDisplayName(this Type enumValue, string value)
        {
            return enumValue
                .GetMember(value)
                .First()
                .GetCustomAttribute<DisplayAttribute>()
                ?.GetName();
        }
    }
}
