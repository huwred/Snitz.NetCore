using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace SnitzCore.Data.Extensions
{
    public static class SettingsHelpers
    {
        public static void AddOrUpdateAppSetting<T>(string sectionPathKey, T value)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true
                };
                var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                string json = File.ReadAllText(filePath);
                var jsonObj = JsonSerializer.Deserialize<Dictionary<string, object>>(json, options);
                if (jsonObj is null) return;

                jsonObj = SetValueRecursively(sectionPathKey, jsonObj,options, value);

                var updatedConfigJson = JsonSerializer.Serialize(jsonObj, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, updatedConfigJson);

            }
            catch (Exception ex)
            {
                //Console.WriteLine("Error writing app settings | {0}", ex.Message);
            }
        }

        private static Dictionary<string, object> SetValueRecursively<T>(string key,
            Dictionary<string, object> config, JsonSerializerOptions options , T value)
        {
            if (key.Contains(":"))
            {
                var subKey = key.Split(':',2);
                if (config.ContainsKey(subKey[0]))
                {
                    var subConfigString = config[subKey[0]].ToString();
                    if (subConfigString is null)
                        return config;
                    var subConfig = JsonSerializer.Deserialize<Dictionary<string, object>>(subConfigString, options);
                    if (subConfig is not null)
                    {
                        var test = SetValueRecursively(subKey[1],subConfig, options,  value);
                        config[subKey[0]] = test;
                        return config;
                    }
                }
                else
                {
                    var test = SetValueRecursively(subKey[1],new Dictionary<string, object>(), options,  value);
                    config[subKey[0]] = test;
                    return config;
                }
            }
            else
                config[key] = value?.ToString() ?? "";
            return config;
        }
    }

}
