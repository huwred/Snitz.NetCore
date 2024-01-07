using System.IO;
using System.Xml.Serialization;

namespace SnitzCore.Data.Extensions;

public static class XmlExtensions
{
    public static T DeserializeListFromXml<T>(string filePath)
    {
        XmlSerializer serializer = new(typeof(T));
        using FileStream fileStream = new(filePath, FileMode.Open);
        return (T)serializer.Deserialize(fileStream)!;
    }
    
    public static bool ContainsAny(this string str, params string[] values)
    {
        if (!string.IsNullOrEmpty(str) || values.Length > 0)
        {
            foreach (string value in values)
            {
                if(str.Contains(value))
                    return true;
            }
        }

        return false;
    }

    public static bool ContainsAll(this string str, params string[] values)
    {
        if (!string.IsNullOrEmpty(str) || values.Length > 0)
        {
            var tomatch = values.Length;
            var matched = 0;
            foreach (string value in values)
            {
                if(str.Contains(value))
                    matched += 1;
            }

            if (matched == tomatch)
            {
                return true;
            }
        }

        return false;
    }
}