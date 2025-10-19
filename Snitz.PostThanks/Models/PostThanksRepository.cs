using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Snitz.PostThanks.Models;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System.Runtime.CompilerServices;

namespace PostThanks.Models
{
    public class PostThanksRepository : IDisposable
    {
        private int? _memberid;
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
            _memberid = memberservice.Current()?.Id;
        }
        public void Dispose()
        {

        }

        /// <summary>
        /// Determines whether the specified topic is enabled for the "Thanks" feature.
        /// </summary>
        /// <remarks>This method checks if the specified topic exists in the forums where the "Thanks"
        /// feature is allowed.</remarks>
        /// <param name="topicid">The unique identifier of the topic to check.</param>
        /// <returns><see langword="true"/> if the topic is enabled for the "Thanks" feature; otherwise, <see langword="false"/>.</returns>
        public bool EnabledForTopic(int topicid)
        {
                var test =  _snitzContext.Forums
                .FromSqlInterpolated($"SELECT * FROM FORUM_FORUM WHERE F_ALLOWTHANKS=1");
            return test.Any(f=>f.Id == topicid);
        }

        /// <summary>
        /// Retrieves a collection of post thanks entries associated with the specified topic ID.
        /// </summary>
        /// <remarks>The returned collection is retrieved without tracking changes in the database
        /// context.</remarks>
        /// <param name="id">The unique identifier of the topic for which to retrieve post thanks entries.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="PostThanksEntry"/> objects representing the post thanks
        /// entries for the specified topic. Returns an empty collection if no entries are found.</returns>
        public IEnumerable<PostThanksEntry> Get(int id)
        {
            return _dbContext.PostThanks.AsNoTracking().Where(e=>e.TopicId == id).AsEnumerable();
        }

        /// <summary>
        /// Adds a "thanks" entry for a specified topic or reply.
        /// </summary>
        /// <remarks>This method records a "thanks" entry in the database for the current member. If the
        /// member ID is null, the operation is not performed, and an error is logged.</remarks>
        /// <param name="topicid">The identifier of the topic to which the "thanks" is being added.</param>
        /// <param name="replyid">The identifier of the reply to which the "thanks" is being added. Defaults to 0 if the "thanks" is for the
        /// topic as a whole.</param>
        public void AddThanks(int topicid, int replyid = 0)
        {
            if(_memberid == null)
            {
                _logger.Error("Member ID is null, cannot add thanks.");
                return;
            }
            try
            {
                var entity = new PostThanksEntry { MemberId = _memberid.Value,TopicId = topicid,ReplyId=replyid };
                _dbContext.PostThanks.Add(entity);
                _dbContext.SaveChanges();

            }
            catch (Exception e)
            {
                var test = e.Message;
            }
        }

        /// <summary>
        /// Determines whether the specified topic belongs to a forum where "thanks" are allowed.
        /// </summary>
        /// <param name="topicid">The unique identifier of the topic to check.</param>
        /// <returns><see langword="true"/> if the topic belongs to a forum that allows "thanks"; otherwise, <see
        /// langword="false"/>.</returns>
        public bool IsAllowedForum(int topicid)
        {

            var forums = _snitzContext.Forums
                .FromSqlInterpolated($"SELECT FORUM_ID FROM {_tableprefix}FORUM WHERE F_ALLOWTHANKS=1");
            var topic = _snitzContext.Posts.SingleOrDefault(p=>p.Id == topicid);
            return forums.Any(f=>f.Id == topic.ForumId);
        }

        /// <summary>
        /// Deletes a "thanks" entry associated with the specified topic and optional reply.
        /// </summary>
        /// <remarks>This method requires the current member ID to be set. If the member ID is null, the
        /// operation will fail, and <see langword="false"/> will be returned.</remarks>
        /// <param name="topicid">The identifier of the topic for which the "thanks" entry should be deleted.</param>
        /// <param name="replyid">The identifier of the reply for which the "thanks" entry should be deleted. Defaults to 0 if not specified.</param>
        /// <returns><see langword="true"/> if the "thanks" entry was successfully deleted; otherwise, <see langword="false"/>.</returns>
        public bool DeleteThanks(int topicid, int replyid = 0)
        {
            if(_memberid == null)
            {
                _logger.Error("Member ID is null, cannot delete thanks.");
                return false;
            }
            try
            {
                var entity = new PostThanksEntry { MemberId = _memberid.Value,TopicId = topicid,ReplyId=replyid };
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

        /// <summary>
        /// Determines whether a specific topic or reply has been thanked.
        /// </summary>
        /// <param name="topicid">The unique identifier of the topic to check.</param>
        /// <param name="replyid">The unique identifier of the reply to check. Defaults to 0, which indicates the topic itself.</param>
        /// <returns><see langword="true"/> if the specified topic or reply has been thanked; otherwise, <see langword="false"/>.</returns>
        public bool IsThanked(int topicid, int replyid = 0)
        {
            if (_dbContext.PostThanks.SingleOrDefault(c => c.TopicId == topicid && c.ReplyId == replyid) != null)
                return true;

            return false;
        }

        /// <summary>
        /// Counts the number of "thanks" associated with a specific topic and reply.
        /// </summary>
        /// <param name="id">The unique identifier of the topic.</param>
        /// <param name="replyid">The unique identifier of the reply within the topic.</param>
        /// <returns>The total number of "thanks" for the specified topic and reply.</returns>
        public int Count(int id, int replyid)
        {
            return _dbContext.PostThanks.Count(t=> t.TopicId == id && t.ReplyId == replyid);

        }

        /// <summary>
        /// Retrieves the total number of "thanks" received by a specific member.
        /// </summary>
        /// <remarks>This method queries the database to calculate the number of "thanks" received by the
        /// member, based on their contributions as a topic author or reply author.</remarks>
        /// <param name="memberid">The unique identifier of the member whose "thanks" count is to be retrieved.</param>
        /// <returns>The total count of "thanks" received by the specified member, including those associated with topics and
        /// replies.</returns>
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

        /// <summary>
        /// Retrieves the count of posts for which the specified member has given thanks.
        /// </summary>
        /// <param name="memberid">The unique identifier of the member whose post thanks count is to be retrieved.</param>
        /// <returns>The total number of posts for which the specified member has given thanks.</returns>
        public int MemberCountGiven(int memberid)
        {
            return _dbContext.PostThanks.Count(t=> t.MemberId == memberid);
        }

        /// <summary>
        /// Retrieves a list of member names who have expressed thanks for a specific topic and reply.
        /// </summary>
        /// <remarks>This method queries the database to find members associated with the specified topic
        /// and reply. Ensure that the provided <paramref name="id"/> and <paramref name="replyid"/> correspond to valid
        /// entries in the database.</remarks>
        /// <param name="id">The unique identifier of the topic.</param>
        /// <param name="replyid">The unique identifier of the reply within the topic.</param>
        /// <returns>A list of member names who have expressed thanks for the specified topic and reply. Returns an empty list if
        /// no members are found.</returns>
        public List<string> Members(int id, int replyid)
        {
            return _dbContext.PostThanks.Include(t=>t.Member).Where(t=> t.TopicId == id && t.ReplyId == replyid).Select(t=>t.Member.Name).ToList();

        }

        /// <summary>
        /// Determines whether the current user is the author of a specified post or reply.
        /// </summary>
        /// <remarks>This method verifies authorship by comparing the user ID of the current user with the
        /// author ID associated with the specified post or reply. The method supports both active and archived
        /// data.</remarks>
        /// <param name="id">The unique identifier of the post to check.</param>
        /// <param name="replyid">The unique identifier of the reply to check. Specify 0 if checking a post.</param>
        /// <param name="archived">A value indicating whether the post or reply is in the archived state. If <see langword="true"/>, the method
        /// checks archived data; otherwise, it checks active data.</param>
        /// <returns><see langword="true"/> if the current user is the author of the specified post or reply; otherwise, <see
        /// langword="false"/>.</returns>
        internal bool IsAuthor(int id, int replyid, bool archived)
        {
            if(archived)
            {
                if (replyid == 0)
                {
                    return _snitzContext.ArchivedTopics.Single(p=>p.ArchivedPostId == id).MemberId == _memberid;
                }
                else
                {
                    return _snitzContext.ArchivedPosts.Single(p=>p.Id == replyid).MemberId == _memberid;
                }
            }
            if (replyid == 0)
            {
                return _snitzContext.Posts.Single(p=>p.Id == id).MemberId == _memberid;
            }
            else
            {
                return _snitzContext.Replies.Single(p=>p.Id == replyid).MemberId == _memberid;
            }

        }

        /// <summary>
        /// Updates the "Allow Thanks" setting for a specific forum.
        /// </summary>
        /// <remarks>This method modifies the database directly to update the "Allow Thanks" setting for
        /// the specified forum. Ensure that the provided <paramref name="id"/> corresponds to a valid forum
        /// entry.</remarks>
        /// <param name="id">The unique identifier of the forum to update.</param>
        /// <param name="v">The value to set for the "Allow Thanks" setting. Typically, <see langword="1"/> to enable or <see
        /// langword="0"/> to disable.</param>
        internal void SetAllowThanks(int id, int v)
        {
            _snitzContext.Database.ExecuteSqlAsync($"Update FORUM_FORUM Set F_ALLOWTHANKS = {v} Where FORUM_ID={id}");
        }
    }

}