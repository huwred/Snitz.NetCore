using Microsoft.AspNetCore.Http;
using SnitzCore.Data.Interfaces;
using System;
using System.Collections.Generic;

namespace SnitzCore.Data.Models
{
    /// <summary>
    /// Snitz Cookie functions
    /// </summary>
    public class SnitzCookie : ISnitzCookie
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISnitzConfig _snitzConfig;

        private readonly List<string> _cookieCollection = new()
        {
            "snitztheme", "SnitzCulture", "pmModPaging", "User",
            "ActiveRefresh", "SinceDate", "TopicTracking", "GROUP", "RefreshFilter",
            "timezoneoffset", "preservedurl", "User"
        };

        public SnitzCookie(IHttpContextAccessor httpContextAccessor,ISnitzConfig snitzConfig)
        {
            _httpContextAccessor = httpContextAccessor;
            _snitzConfig = snitzConfig;
            string? snitzUniqueId = snitzConfig.UniqueId;
            if (snitzUniqueId != null) _cookieCollection.Add(snitzUniqueId);
        }
        public  void LogOut()
        {
            Clear();
        }

        public void Clear(string? value = null)
        {
            if (value != null)
            {
                ExpireCookie(value);
            }
            else
            {
                foreach (string s in _cookieCollection)
                {
                    ExpireCookie(s);
                }
            }

        }

        public  void ClearAll()
        {
            var myCookies = _httpContextAccessor.HttpContext?.Request.Cookies.Keys;
            if (myCookies != null)
            {
                foreach (string cookie in myCookies)
                {
                    ExpireCookie(cookie);
                }
            }
        }
        #region LastVisitDate
        public  string? GetLastVisitDate()
        {
            return GetCookieValue("LastVisit");
        }

        public  void SetLastVisitCookie(string? value)
        {
            if (value == null)
                return;
            CookieOptions options = new()
            {
                HttpOnly = false,
                Secure = true,
                Path = _snitzConfig.CookiePath,
                Expires = DateTime.UtcNow.AddMonths(2),
                Domain = _httpContextAccessor.HttpContext?.Request.Host.Host
            };
            

            _httpContextAccessor.HttpContext?.Response.Cookies.Append("LastVisit", value, options);
        }
        #endregion

        #region Poll vote tracker
        //public  void PollVote(int pollid)
        //{
        //    Dictionary<string, string> pages = GetMultipleUsingSingleKeyCookies("votetracker");

        //    if (pages.ContainsKey(pollid.ToString()))
        //    {
        //        int lastpage = Convert.ToInt32(pages[pollid.ToString()]);
        //        pages[pollid.ToString()] = "1";
        //    }
        //    else
        //    {
        //        pages.Add(pollid.ToString(), "1");
        //    }
        //    SetMultipleUsingSingleKeyCookies("votetracker", pages);
        //}
        //public  bool HasVoted(int pollid)
        //{
        //    var pages = GetMultipleUsingSingleKeyCookies("votetracker");
        //    if (pages.ContainsKey(pollid.ToString()))
        //    {
        //        return true;
        //    }
        //    return false;
        //}
        #endregion


        #region Active Topic cookies
        public  string? GetActiveRefresh()
        {
            return GetCookieValue("ActiveRefreshSeconds");
        }

        public  void SetActiveRefresh(string value)
        {
            SetCookie("ActiveRefreshSeconds", value,DateTime.Now.AddMonths(1));
        }

        public  string? GetTopicSince()
        {
            return GetCookieValue("SinceDateEnum");
        }

        public  void SetTopicSince(string value)
        {
            SetCookie("SinceDateEnum", value,DateTime.Now.AddMonths(1));
        }


        #endregion

        #region private methods

        public  string? GetCookieValue(string cookieKey)
        {
            var cookie = _httpContextAccessor.HttpContext?.Request.Cookies[cookieKey];
            return cookie;
        }


        public  void SetCookie(string name, string value, DateTime? expires = null)
        {
            if (!_cookieCollection.Contains(name))
            {
                _cookieCollection.Add(name);
            }

            CookieOptions options = new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                Path = _snitzConfig.CookiePath,
                Expires = expires ?? DateTime.UtcNow,
                Domain = _httpContextAccessor.HttpContext?.Request.Host.Host
            };
            _httpContextAccessor.HttpContext?.Response.Cookies.Append(name, value, options);
        }

        private  void ExpireCookie(string? name)
        {

            if (name != null && _httpContextAccessor.HttpContext?.Request.Cookies[name] != null)
            {
                _httpContextAccessor.HttpContext.Response.Cookies.Delete(name); 
            }

        }

        #endregion

    }
}
