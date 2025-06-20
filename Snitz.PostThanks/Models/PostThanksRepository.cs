

using BbCodeFormatter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using Snitz.PhotoAlbum.Models;
using Snitz.PostThanks.Models;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System.Runtime.CompilerServices;

namespace PostThanks.Models
{
    public class PostThanksRepository : IDisposable
    {
        private int _memberid;
        private readonly string? _tableprefix;

        private readonly PostThanksContext _dbContext;
        private readonly ISnitzConfig _config;
        private readonly SnitzDbContext _snitzContext;
        protected static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public PostThanksRepository(PostThanksContext dbContext,ISnitzConfig config,SnitzDbContext snitzContext,IOptions<SnitzForums> options,IMember memberservice)
        {
            _dbContext = dbContext;
            _config = config;
            _snitzContext = snitzContext;
            _tableprefix = options.Value.forumTablePrefix;
            _memberid = memberservice.Current().Id;
        }
        public void Dispose()
        {

        }


        public void AddThanks(int topicid, int replyid = 0)
        {
            try
            {
                var entity = new PostThanksEntry { MemberId = _memberid,TopicId = topicid,ReplyId=replyid };
                _dbContext.PostThanks.Add(entity);
                _dbContext.SaveChanges();

            }
            catch (Exception e)
            {
                var test = e.Message;
            }
        }
        public bool IsAllowedForum(int topicid)
        {

            var forums = _snitzContext.Forums
                .FromSqlInterpolated($"SELECT FORUM_ID FROM {_tableprefix}FORUM WHERE F_ALLOWTHANKS=1");
            var topic = _snitzContext.Posts.SingleOrDefault(p=>p.Id == topicid);
            return forums.Any(f=>f.Id == topic.ForumId);
        }
        public bool DeleteThanks(int topicid, int replyid = 0)
        {
            try
            {
                var entity = new PostThanksEntry { MemberId = _memberid,TopicId = topicid,ReplyId=replyid };
                _dbContext.PostThanks.Remove(entity);
                _dbContext.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }

        }

        public bool IsThanked(int topicid, int replyid = 0)
        {
            if (_dbContext.PostThanks.SingleOrDefault(c => c.TopicId == topicid && c.ReplyId == replyid) != null)
                return true;

            return false;
        }

        public int Count(int id, int replyid)
        {
            return _dbContext.PostThanks.Count(t=> t.TopicId == id && t.ReplyId == replyid);

        }

        public int MemberCountReceived(int memberid)
        {
            var sqlString = $@"SELECT t.T_AUTHOR,r.R_AUTHOR FROM {_tableprefix}THANKS ft
                LEFT JOIN {_tableprefix}TOPICS t on ft.TOPIC_ID = t.TOPIC_ID
                LEFT JOIN {_tableprefix}REPLY r on ft.REPLY_ID = r.REPLY_ID
                WHERE t.T_AUTHOR = {memberid} OR r.R_AUTHOR  = {memberid}";

            var count = _snitzContext.Forums
                .FromSqlInterpolated(FormattableStringFactory.Create(sqlString)).Count();

            return count;

        }

        public int MemberCountGiven(int memberid)
        {
            return _dbContext.PostThanks.Count(t=> t.MemberId == memberid);
        }

        public List<string> Members(int id, int replyid)
        {
            return _dbContext.PostThanks.Include(t=>t.Member).Where(t=> t.TopicId == id && t.ReplyId == replyid).Select(t=>t.Member.Name).ToList();

        }

        internal bool IsAuthor(int id, int replyid)
        {
            if(replyid == 0)
            {
                return _snitzContext.Posts.Single(p=>p.Id == id).MemberId == _memberid;
            }
            else
            {
                return _snitzContext.Replies.Single(p=>p.Id == replyid).MemberId == _memberid;
            }

        }
    }

}