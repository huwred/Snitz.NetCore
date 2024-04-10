using SnitzCore.Data.Models;
using System.Collections.Generic;

namespace SnitzCore.Data.Interfaces
{
    public interface ISnitz
    {
        ForumTotal Totals();
        LastPostViewModel LastPost();
        IEnumerable<SnitzConfig> GetConfig();
        int ForumCount();
        IEnumerable<KeyValuePair<int, string>> GetForumModerators();

        string NewestMember();
        int ActiveSince(string lastvisit);
    }
}
