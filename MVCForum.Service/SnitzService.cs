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
            var result = _dbContext.ForumTotal.AsNoTracking().OrderBy(t=>t.Id).First();
            var activemembers = _dbContext.Members.Where(m => m.Status == 1 && m.Posts > 0).Count();
            result.ActiveMembers = activemembers;
            return result;
        }

        public string NewestMember()
        {
            return _dbContext.Members.AsNoTracking().OrderByDescending(m => m.Created).First(m=>m.Status == 1).Name;
        }

        public LastPostViewModel LastPost()
        {
            try
            {
                var lastpost = _dbContext.Forums.AsNoTracking().OrderByDescending(f=>f.LastPost).First();
                var lastreply = _dbContext.Replies.AsNoTracking().Include(f=>f.Member).SingleOrDefault(p=>p.Id == lastpost.LatestReplyId);
                var lasttopic = _dbContext.Posts.AsNoTracking().Include(f=>f.Member).Single(p=>p.Id == lastpost.LatestTopicId);

                dynamic latest = lastreply?.Created.FromForumDateStr() > lasttopic.Created.FromForumDateStr() ? lastreply : lasttopic;

                if (latest is Post)
                {
                
                    return new LastPostViewModel()
                    {
                        LastPostDate = latest.Created,
                        LastPostAuthor = latest.Member.Id,
                        LastTopic = latest.Id
                    };
                }
                return new LastPostViewModel()
                {
                    LastPostDate = latest.Created,
                    LastPostAuthor = latest.Member.Id,
                    LastTopic = latest.PostId,
                    LastReply = latest.Id
                };
            }
            catch (System.Exception)
            {
                return new LastPostViewModel();
            }

        }

        public int ForumCount()
        {
            return _dbContext.Forums.Count(f=>f.Status == 1);
        }

        public IEnumerable<SnitzConfig> GetConfig()
        {
            return _dbContext.SnitzConfig.AsNoTracking().AsQueryable();
        }

        public IEnumerable<KeyValuePair<int, string>> GetForumModerators()
        {
            return _dbContext.Members.AsNoTracking().Where(m=>m.Level>1).ToDictionary(o => o.Id, o => o.Name).ToList();
        }

        public int ActiveSince(string lastvisit)
        {
            //use string.compare because raw dates are stored as strings
            return _dbContext.Posts.AsNoTracking().Where(t=> string.Compare(t.LastPostDate, lastvisit) > 0).Count();
        }

        public int MembersSince(string lastvisit)
        {
            return _dbContext.Members.AsNoTracking().Where(t=> string.Compare(t.Lastactivity, lastvisit) > 0).Count();
        }
        public int SignupsSince(string lastvisit)
        {
            return _dbContext.Members.AsNoTracking().Where(t=> string.Compare(t.Lastactivity, lastvisit) > 0).Count();
        }
    }


}
