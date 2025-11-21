using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using SnitzCore.Service.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
            get => _httpContextAccessor.HttpContext?.Request.PathBase;
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
        string? ISnitzConfig.Copyright
        {
            get => _config.GetSection("SnitzForums").GetSection("strCopyright").Value;
            set => AddOrUpdateAppSetting("strCopyright",value);
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
                var stringCaptcha = GetValueWithDefault("STRCAPTCHAOPERATORS","");
                char sep = ';';
                if (stringCaptcha.Contains(","))
                {
                    sep = ',';
                }

                List<CaptchaOperator> operators = new List<CaptchaOperator>();
                if (GetValueWithDefault("STRCAPTCHAOPERATORS","") != "")
                {
                    var arrCaptcha = stringCaptcha.Split(new[] { sep },StringSplitOptions.RemoveEmptyEntries);
                    foreach (string s in arrCaptcha)
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
                        if (AppBaseUrl == "") 
                            AppBaseUrl = "/";
                        return AppBaseUrl;
                    }
                }
                return "/";
            }
        }

        private readonly List<SnitzConfig> _cachedConfig = new List<SnitzConfig>();

        string? ISnitzConfig.ForumDescription
        {
            get => _config.GetSection("SnitzForums").GetSection("strForumDescription").Value; 
            set => AddOrUpdateAppSetting("strForumDescription",value);
        }
        public ConfigService(SnitzDbContext dbContext,IConfiguration config,IHttpContextAccessor httpContextAccessor,IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _env = env;

            _cachedConfig = CacheProvider.GetOrCreate("snitzconfig_cache",()=> _dbContext.SnitzConfig.AsNoTracking().ToList(),TimeSpan.FromHours(1));
        }

        public void RemoveFromCache(string key)
        {
            CacheProvider.Remove("cfg_" + key);
        }
        public int GetIntValue(string key, int defaultvalue = 0)
        {
            var config = _cachedConfig.SingleOrDefault(c => c.Key == key);
            if (config != null && int.TryParse(config.Value, out int parsedValue))
            {
                return parsedValue;
            }

            return defaultvalue;
        }
        public bool IsEnabled(string key)
        {
            var config = _cachedConfig.SingleOrDefault(c => c.Key == key);
            if (config != null && int.TryParse(config.Value, out int keyvalue))
            {
                return keyvalue == 1;
            }
            return false;
        }
        public IEnumerable<string> GetRequiredMemberFields()
        {
            return _cachedConfig.Where(c => c.Key.StartsWith("STRREQ") && c.Value == "1").Select(c=>c.Key);
        }


        public string GetValue(string key)
        {
            return GetValueWithDefault(key, "");
            //return _dbContext.SnitzConfig.SingleOrDefault(c=>c.Key == key)?.Value ?? "";
        }
        public string GetValueWithDefault(string key, string? defVal = null)
        {
            var result = _cachedConfig.SingleOrDefault(c=>c.Key == key);
            return result != null ? result.Value : defVal!;
        }

        public bool TableExists(string tablename)
        {
            return _dbContext.TableExists(tablename).Result;
        }

        public IEnumerable<Badword> GetBadwords()
        {
            return CacheProvider.GetOrCreate("badword_cache",()=> _dbContext.Badwords.ToList(),TimeSpan.FromHours(1));
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
                //Console.WriteLine("Error writing app settings");
            }
        }

        public string ThemeCss(string cssfile)
        {
            var currentTheme = _httpContextAccessor.HttpContext?.Request.Cookies["snitztheme"] ?? GetValueWithDefault("STRDEFAULTTHEME", "SnitzTheme");
            var root = _httpContextAccessor.HttpContext?.Request.PathBase;
            return $"{root}/css/Themes/{currentTheme}/{cssfile}";
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
                _dbContext.SnitzConfig.Update(configval);
            }

            _dbContext.SaveChanges();
            CacheProvider.Remove("snitzconfig_cache");

        }
    }
}
