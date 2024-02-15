using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SnitzCore.Data.Extensions;

namespace SnitzCore.Service
{
    public class SnitzService : ISnitz
    {
        private readonly SnitzDbContext _dbContext;

        public SnitzService(SnitzDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public ForumTotal Totals()
        {
            return _dbContext.ForumTotal.First();
        }

        public LastPostViewModel LastPost()
        {
            var lastpost = _dbContext.Forums.OrderByDescending(f=>f.LastPost).First();
            var lastreply = _dbContext.Replies.Include(f=>f.Member).Single(p=>p.Id == lastpost.LatestReplyId);
            var lasttopic = _dbContext.Posts.Include(f=>f.Member).Single(p=>p.Id == lastpost.LatestTopicId);

            dynamic latest = lastreply.Created.FromForumDateStr() > lasttopic.Created.FromForumDateStr() ? lastreply : lasttopic;
            return new LastPostViewModel()
            {
                LastPostDate = latest.Created,
                LastPostAuthor = latest.Member.Id,
                LastTopic = latest.PostId ?? latest.Id,
                LastReply = latest.Id
            };
        }
        public int ForumCount()
        {
            return _dbContext.Forums.Count(f=>f.Status == 1);
        }
        public IEnumerable<SnitzConfig> GetConfig()
        {
            return _dbContext.SnitzConfig.AsQueryable();
        }

        public IEnumerable<KeyValuePair<int, string>> GetForumModerators()
        {
            return _dbContext.Members.Where(m=>m.Level>1).ToDictionary(o => o.Id, o => o.Name).ToList();
        }
    }


}
