﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using SnitzCore.Service.Extensions;

namespace SnitzCore.Service
{
    public class ConfigService : ISnitzConfig
    {
        private readonly SnitzDbContext _dbContext;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _env;

        string? ISnitzConfig.RootFolder
        {
            get => _httpContextAccessor.HttpContext?.Request.PathBase == "/" ? "" : _httpContextAccessor.HttpContext?.Request.PathBase;
        }
        string ISnitzConfig.ContentFolder
        {
            get => GetIntValue("INTPROTECTCONTENT") == 1 ? "ProtectedContent" : "Content";
            set => AddOrUpdateAppSetting("INTPROTECTCONTENT",value);
        }
        string? ISnitzConfig.ForumTitle
        {
            get => _config.GetSection("SnitzForums").GetSection("strForumTitle").Value;
            set => AddOrUpdateAppSetting("strForumTitle",value);
        }
        public string? UniqueId
        {
            get => _config.GetSection("SnitzForums").GetSection("strUniqueId").Value; 
            set => AddOrUpdateAppSetting("strUniqueId",value);
        }
        public int DefaultPageSize
        {
            get => GetIntValue("STRPAGESIZE",25); 
            set => AddOrUpdateAppSetting("STRPAGESIZE",value);
        }

        public IEnumerable<CaptchaOperator> CaptchaOperators {             
            get
            {
                List<CaptchaOperator> operators = new List<CaptchaOperator>();
                if (GetValue("STRCAPTCHAOPERATORS","") != "")
                {
                    var stringCaptcha = GetValue("STRCAPTCHAOPERATORS","").Split(new[] { ';' },
                        StringSplitOptions.RemoveEmptyEntries);
                    foreach (string s in stringCaptcha)
                    {
                        operators.Add((CaptchaOperator)Enum.Parse(typeof(CaptchaOperator), s));
                    }
                }
                return operators;
            }
            set
            {
                var strcap = "";
                foreach (var captchaOperator in value)
                {
                    if (strcap != "")
                        strcap += ";";
                    strcap += captchaOperator.ToString();
                }
                AddOrUpdateAppSetting("STRCAPTCHAOPERATORS",strcap);
            }
        }

        string? ISnitzConfig.ForumUrl
        {
            get => _config.GetSection("SnitzForums").GetSection("strForumUrl").Value; 
            set => AddOrUpdateAppSetting("strForumUrl",value);
        }

        public string DateTimeFormat { get
            {
    ;
                return "yyyyMMddHHmmss";
            }
        }
        public string DateFormat { get
            {
                ;
                return "yyyyMMdd";
            }
        }
        public string? CookiePath
        {
            get
            {
                if (GetIntValue("STRSETCOOKIETOFORUM") == 1)
                {
                    if (_httpContextAccessor.HttpContext != null)
                    {
                        HttpContext Current = _httpContextAccessor.HttpContext;
                        string AppBaseUrl = $"{Current.Request.PathBase}";
                        if (AppBaseUrl != "/") 
                            AppBaseUrl = "/" + AppBaseUrl;
                        return AppBaseUrl;
                    }
                }
                return "/";
            }
        }

        public ConfigService(SnitzDbContext dbContext,IConfiguration config,IHttpContextAccessor httpContextAccessor,IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _env = env;

        }

        public void RemoveFromCache(string key)
        {
            var service = new InMemoryCache();
            service.Remove("cfg_" + key);
        }
        public int GetIntValue(string? key, int defaultvalue = 0)
        {
            if(string.IsNullOrWhiteSpace(key)) return defaultvalue;
            var service = new InMemoryCache() { DoNotExpire = true };
            return int.Parse(service.GetOrSet("cfg_" + key, () => CachedIntValue(key,defaultvalue))!);

        }

        public IEnumerable<string> GetRequiredMemberFields()
        {
            return _dbContext.SnitzConfig.Where(c => c.Key.StartsWith("STRREQ") && c.Value == "1").Select(c=>c.Key);
        }
        private string? CachedIntValue(string? key, int defaultvalue)
        {
            var config = _dbContext.SnitzConfig.SingleOrDefault(c => c.Key == key);
            if (config != null)
            {
                    return config.Value;
            }

            return defaultvalue.ToString();
        }

        public string GetValue(string key)
        {
            return _dbContext.SnitzConfig.SingleOrDefault(c=>c.Key == key)?.Value ?? "";
        }
        public string GetValue(string key, string? defVal = null)
        {
            var service = new InMemoryCache() { DoNotExpire = true };
            return service.GetOrSet("cfg_" + key, () => CachedStringValue(key,defVal))!;

        }

        private string CachedStringValue(string key, string? defVal)
        {
            var result = _dbContext.SnitzConfig.SingleOrDefault(c=>c.Key == key)?.Value;
            return result ?? defVal!;
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
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json)!;
                
                var sectionPath = key.Split(":")[0];

                if (!string.IsNullOrEmpty(sectionPath))
                {
                    var keyPath = key.Split(":")[1];
                    if (jsonObj != null) jsonObj[sectionPath][keyPath] = value;
                }
                else
                {
                    if (jsonObj != null) jsonObj[sectionPath] = value; // if no sectionpath just set the value
                }

                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(filePath, output);
            }
            catch (ConfigurationErrorsException) 
            {
                Console.WriteLine("Error writing app settings");
            }
        }

        public string ThemeCss(string cssfile)
        {
            //var currentTheme = _snitzcookie.GetCookieValue("snitztheme") ?? GetValue("STRDEFAULTTHEME", "SnitzTheme");
            var currentTheme = GetValue("STRDEFAULTTHEME", "SnitzTheme");
            return $"/css/themes/{currentTheme}/{cssfile}";
        }

        public void SetValue(string key, string value)
        {
            var configval = _dbContext.SnitzConfig.SingleOrDefault(c=>c.Key == key);
            if (configval == null)
            {
                _dbContext.SnitzConfig.Add(new SnitzConfig()
                {
                    Key = key,
                    Value = value
                });
            }
            else
            {
                configval.Value = value;
            }

            _dbContext.SaveChanges();
        }
    }
}
