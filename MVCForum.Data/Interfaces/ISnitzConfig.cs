using SnitzCore.Data.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SnitzCore.Data.Interfaces
{
    public interface ISnitzConfig
    {
        string CookiePath { get; set; }
        string ForumUrl { get; set; }
        string RootFolder { get; }
        string ContentFolder { get; set; }
        string ForumTitle { get; set; }
        string UniqueId { get; set; }
        int DefaultPageSize { get; set; }
        IEnumerable<CaptchaOperator> CaptchaOperators { get; set; }
        IEnumerable<string> GetRequiredMemberFields();
        int GetIntValue(string key, int defaultvalue = 0);
        string GetValue(string key);
        string? GetValue(string key,string? defVal = null);
        bool TableExists(string tablename);

        IEnumerable<Badword> GetBadwords();

    }
}
