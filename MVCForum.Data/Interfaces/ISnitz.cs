using SnitzCore.Data.Models;
using System.Collections.Generic;

namespace SnitzCore.Data.Interfaces
{
    public interface ISnitz
    {
        ForumTotal Totals();
        Post LastPost();
        IEnumerable<SnitzConfig> GetConfig();
        int ForumCount();
        IEnumerable<KeyValuePair<int, string>> GetForumModerators();
    }
}
