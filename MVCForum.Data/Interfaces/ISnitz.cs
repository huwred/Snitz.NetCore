using SnitzCore.Data.Models;
using System.Collections.Generic;

namespace SnitzCore.Data.Interfaces
{
    public interface ISnitz
    {
        /// <summary>
        /// Get Forum Totals (Post, Members etc)
        /// </summary>
        /// <returns>ForumTotal</returns>
        ForumTotal Totals();

        /// <summary>
        /// Fetch the latest post
        /// </summary>
        /// <returns>LastPostViewModel</returns>
        LastPostViewModel LastPost();

        /// <summary>
        /// Get the Forum Config from database
        /// </summary>
        /// <returns>Data from FORUM_CONFIG_NEW</returns>
        IEnumerable<SnitzConfig> GetConfig();

        /// <summary>
        /// Count of Forums
        /// </summary>
        /// <returns></returns>
        int ForumCount();

        /// <summary>
        /// Fetch a list of Moderators
        /// </summary>
        /// <returns></returns>
        IEnumerable<KeyValuePair<int, string>> GetForumModerators();

        /// <summary>
        /// Get the newest member
        /// </summary>
        /// <returns>Username of latest member</returns>
        string NewestMember();

        /// <summary>
        /// Fetch the count of active topics since a specific date
        /// </summary>
        /// <param name="lastvisit">Date parameter as ForumDateStr</param>
        /// <returns>Number of posts</returns>
        int ActiveSince(string lastvisit);
    }
}
