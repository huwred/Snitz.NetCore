using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace SnitzCore.Service
{
    public class ConfigService : ISnitzConfig
    {
        private readonly SnitzDbContext _dbContext;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public string CookiePath
        {
            get => _config.GetSection("SnitzForums").GetSection("strCookiePath").Value; 
            set => AddOrUpdateAppSetting("strCookiePath",value); 
        }
        string ISnitzConfig.RootFolder
        {
            get => _httpContextAccessor.HttpContext?.Request.PathBase == "/" ? "" : _httpContextAccessor.HttpContext?.Request.PathBase;
        }
        string ISnitzConfig.ContentFolder
        {
            get => GetIntValue("INTPROTECTCONTENT", 0) == 1 ? "ProtectedContent" : "Content";
            set => AddOrUpdateAppSetting("INTPROTECTCONTENT",value);
        }
        string ISnitzConfig.ForumTitle
        {
            get => _config.GetSection("SnitzForums").GetSection("strForumTitle").Value;
            set => AddOrUpdateAppSetting("strForumTitle",value);
        }
        public string UniqueId
        {
            get => _config.GetSection("SnitzForums").GetSection("strUniqueId").Value; 
            set => AddOrUpdateAppSetting("strUniqueId",value);
        }
        public int DefaultPageSize
        {
            get => GetIntValue("STRPAGESIZE",25); 
            set => AddOrUpdateAppSetting("STRPAGESIZE",value);
        }

        string ISnitzConfig.ForumUrl
        {
            get => _config.GetSection("SnitzForums").GetSection("strForumUrl").Value; 
            set => AddOrUpdateAppSetting("strForumUrl",value);
        }

        public ConfigService(SnitzDbContext dbContext,IConfiguration config,IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _config = config;
            _httpContextAccessor = httpContextAccessor;

        }
        
        public int GetIntValue(string key, int defaultvalue = 0)
        {
            if(string.IsNullOrWhiteSpace(key)) return defaultvalue;

            var config = _dbContext.SnitzConfig.SingleOrDefault(c => c.Key == key);
            if (config != null)
            {
                if (Int32.TryParse(config.Value, out var configvalue))
                    return configvalue;
            }

            return defaultvalue;
        }

        public string GetValue(string key)
        {
            return _dbContext.SnitzConfig.SingleOrDefault(c=>c.Key == key)?.Value;
        }
        public string GetValue(string key, string? defVal = null)
        {
            var result = _dbContext.SnitzConfig.SingleOrDefault(c=>c.Key == key)?.Value;
            return result ?? defVal;
        }
        public bool TableExists(string tablename)
        {
            bool exists = _dbContext.TableExists(tablename).Result;
            return exists;
        }

        public IEnumerable<Badword> GetBadwords()
        {
            return _dbContext.Badwords;
        }

        private void AddOrUpdateAppSetting<T>(string key, T value) 
        {
            try 
            {
                var filePath = Path.Combine(AppContext.BaseDirectory, "appSettings.json");
                string json = File.ReadAllText(filePath);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                
                var sectionPath = key.Split(":")[0];

                if (!string.IsNullOrEmpty(sectionPath)) 
                {
                    var keyPath = key.Split(":")[1];
                    jsonObj[sectionPath][keyPath] = value;
                }
                else 
                {
                    jsonObj[sectionPath] = value; // if no sectionpath just set the value
                }

                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(filePath, output);
            }
            catch (ConfigurationErrorsException) 
            {
                Console.WriteLine("Error writing app settings");
            }
        }

    }
}
