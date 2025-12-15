using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public static class EfNullCheckHelper
{
    private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

    /// <summary>
    /// Checks each property of each object in the collection for nullability issues.
    /// </summary>
    public static void CheckForNullableValueErrors<T>(IEnumerable<T> items)
    {
        int index = 0;
        foreach (var item in items)
        {
            index++;
            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                try
                {
                    // Access the property value
                    var value = prop.GetValue(item);

                    // If it's a Nullable<T>, check HasValue
                    if (value == null && Nullable.GetUnderlyingType(prop.PropertyType) == null)
                    {
                        Console.Write($"[WARNING] Row {index}, Property '{prop.Name}' is NULL but type is non-nullable.");
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Console.Write($"[ERROR] Row {index}, Property '{prop.Name}' caused: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    Console.Write($"[OTHER ERROR] Row {index}, Property '{prop.Name}' caused: {ex.Message}", ex);
                }
            }
        }
    }
}

