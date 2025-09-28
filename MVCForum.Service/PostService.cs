using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using SnitzCore.Service.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Threading.Tasks;
using X.PagedList;
using X.PagedList.Extensions;
using IMember = SnitzCore.Data.IMember;

namespace SnitzCore.Service
{
    public class PostService : IPost
    {
        private readonly SnitzDbContext _dbContext;
        private readonly IMember _memberService;
        private readonly IForum _forumservice;
        private readonly ISnitzConfig _config;
        private readonly IEmailSender _mailSender;
        private readonly string? _tableprefix;
        private readonly IPrincipal _user;
        private readonly log4net.ILog _logger;

        public PostService(SnitzDbContext dbContext, IMember memberService,IForum forumservice,ISnitzConfig config,IEmailSender mailSender,IOptions<SnitzForums> options,IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _memberService = memberService;
            _forumservice = forumservice;
            _config = config;
            _mailSender = mailSender;
            _tableprefix = options.Value.forumTablePrefix;
            _user = httpContextAccessor.HttpContext?.User;
            _logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType!);
        }

        /// <summary>
        /// Creates a new post in the database and updates related forum and member statistics.
        /// </summary>
        /// <remarks>This method performs several operations: <list type="bullet"> <item>Filters bad words
        /// in the post content if the bad word filter is enabled in the configuration.</item> <item>Sets metadata for
        /// the post, such as the last post date and author.</item> <item>Updates forum statistics, including the last
        /// post and topic count.</item> <item>Updates member statistics, such as post count or last post
        /// information.</item> </list> The method ensures that the post is properly added to the database and related
        /// entities are updated.</remarks>
        /// <param name="post">The post to be created. Must not be null and should contain valid content and metadata.</param>
        /// <returns>The unique identifier of the newly created post.</returns>
        public async Task<int> Create(Post post)
        {
            if (_config.GetIntValue("STRBADWORDFILTER") == 1)
            {
                var badwords = CacheProvider.GetOrCreate("Badwords", () =>_dbContext.Badwords.AsNoTracking()
                    .ToDictionary(t=>t.Word),TimeSpan.FromMinutes(15));
                foreach (var badword in badwords)
                {
                    post.Content.Replace(badword.Key, badword.Value.ReplaceWith);
                }
            }
            post.LastPostDate = post.Created;
            //post.Status = 1;
            post.LastPostAuthorId = post.MemberId;
            await _dbContext.Posts.AddAsync(post);
            await _dbContext.SaveChangesAsync();

            if(post.Status != (int)Status.Draft)
            {
                var forum = _forumservice.UpdateLastPost(post.ForumId);
                if (forum.CountMemberPosts == 1)
                {
                    await _memberService.UpdatePostCount(post.MemberId);
                }
                else
                {
                    await _memberService.UpdateLastPost(post.MemberId);
                }
            
                var forumtotals = _dbContext.ForumTotal.OrderBy(t=>t.Id).First();
                forumtotals.TopicCount += 1;
                await _dbContext.SaveChangesAsync();
                CacheProvider.Remove("AllForums");
            }

            return post.Id;
        }

        /// <summary>
        /// Creates a new reply to a post and updates related forum and member statistics.
        /// </summary>
        /// <remarks>This method performs several operations: <list type="bullet"> <item>Filters the reply
        /// content for inappropriate words if the bad word filter is enabled in the configuration.</item>
        /// <item>Persists the reply to the database.</item> <item>Updates the forum's last post information and member
        /// statistics, including post count or last post details.</item> <item>Increments the total post count for the
        /// forum.</item> </list> The bad word filter is cached for performance optimization and refreshed every 15
        /// minutes.</remarks>
        /// <param name="post">The <see cref="PostReply"/> object representing the reply to be created. The <see cref="PostReply.Content"/>
        /// property may be filtered for inappropriate words based on configuration.</param>
        /// <returns>The unique identifier of the newly created reply.</returns>
        public async Task<int> Create(PostReply post)
        {
            if (_config.GetIntValue("STRBADWORDFILTER") == 1)
            {
                var badwords = CacheProvider.GetOrCreate("Badwords", () =>_dbContext.Badwords.AsNoTracking()
                    .ToDictionary(t=>t.Word),TimeSpan.FromMinutes(15));
                foreach (var badword in badwords)
                {
                    post.Content.Replace(badword.Key, badword.Value.ReplaceWith);
                }
            }
            int? moderated = null;
            if (post.Status == (short)Status.UnModerated || post.Status == (short)Status.OnHold)
            {
                moderated = -1;
            }
            _dbContext.Replies.Add(post);
            await _dbContext.SaveChangesAsync();
            if(post.Status != (int)Status.Draft)
            {
                await UpdateLastPost(post.PostId, moderated);
                var forum = _forumservice.UpdateLastPost(post.ForumId);
                if (forum.CountMemberPosts == 1)
                {
                    await _memberService.UpdatePostCount(post.MemberId);
                }
                else
                {
                    await _memberService.UpdateLastPost(post.MemberId);
                }

                var forumtotals = _dbContext.ForumTotal.OrderBy(t=>t.Id).First();
                forumtotals.PostCount += 1;
                _dbContext.Update(forumtotals);
                await _dbContext.SaveChangesAsync();
            }

            return post.Id;
        }

        /// <summary>
        /// Merges multiple topics into a single main topic and updates related data such as replies, subscriptions, and
        /// forum statistics.
        /// </summary>
        /// <remarks>This method performs the following operations: <list type="bullet"> <item>Identifies
        /// the main topic based on the earliest creation date among the selected topics.</item> <item>Updates replies,
        /// subscriptions, and other related data to associate them with the main topic.</item> <item>Removes the merged
        /// topics from the database.</item> <item>Updates forum statistics and sends notification emails if
        /// configured.</item> </list> Throws an exception if any of the selected topics belong to an invalid
        /// forum.</remarks>
        /// <param name="selected">An array of topic IDs to be merged. If <see langword="null"/> or empty, no topics will be merged.</param>
        /// <returns>The ID of the main topic after the merge.</returns>
        /// <exception cref="Exception">Thrown if a source forum ID associated with one of the topics is invalid.</exception>
        public int CreateForMerge(int[]? selected)
        {
            var topics = _dbContext.Posts.AsNoTracking().Include(t=>t.Member).Where(p => selected != null && EF.Constant(selected).Contains(p.Id)).OrderBy(t => t.Created).ToList();
            int unmoderatedposts = 0;
            int replycounter = 0;
            Post mainTopic = topics.OrderBy(t => t.Created).First();
            int maintopicid = mainTopic.Id;
            unmoderatedposts += mainTopic.UnmoderatedReplies;
            Forum? forum = _dbContext.Forums.Find(mainTopic.ForumId);
            Forum? oldforum = null;
            foreach (Post topic in topics)
            {
                if (topic.Id != mainTopic.Id)
                {
                    //Update the replies with new topic ID and add the number updated to the replycounter
                    var updatereplies = $"UPDATE {_tableprefix}REPLY SET TOPIC_ID={mainTopic.Id},FORUM_ID={mainTopic.ForumId},CAT_ID={mainTopic.CategoryId} WHERE TOPIC_ID={topic.Id}";
                    replycounter += _dbContext.Database.ExecuteSql(FormattableStringFactory.Create(updatereplies));
                    unmoderatedposts += topic.UnmoderatedReplies;
                    //creat a new reply from the topic
                    if (topic.ForumId != mainTopic.ForumId)
                    {
                        oldforum = _dbContext.Forums.Find(topic.ForumId);
                        if (oldforum == null)
                        {
                            throw new Exception("Source FORUM_ID is invalid");
                        }
                    }
                    var reply = new PostReply
                    {
                        CategoryId = mainTopic.CategoryId,
                        ForumId = mainTopic.ForumId,
                        PostId = mainTopic.Id,
                        Created = topic.Created,
                        MemberId = topic.MemberId,
                        Sig = topic.Sig,
                        Content = topic.Content,
                        LastEdited = topic.LastEdit,
                        Status = topic.Status,
                        Ip = topic.Ip
                    };
                    replycounter += 1;
                    _dbContext.Add<PostReply>(reply);
                    _dbContext.Remove(topic);
                    _dbContext.SaveChanges();
                    try
                    {
                        //Update any subscriptions for the old Topic with the new topic ID
                        var subsSql = $"UPDATE {_tableprefix}SUBSCRIPTIONS SET TOPIC_ID={mainTopic.Id}, FORUM_ID={mainTopic.ForumId}, CAT_ID={mainTopic.CategoryId} WHERE TOPIC_ID={topic.Id}";
                        _dbContext.Database.ExecuteSql(FormattableStringFactory.Create(subsSql));

                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    if (oldforum != null)
                    {
                        _forumservice.UpdateLastPost(oldforum.Id);
                    }

                    if (_config.GetIntValue("STRMOVENOTIFY") == 1)
                        _mailSender.TopicMergeEmail(topic, mainTopic, topic.Member!);
                }
            }

            //update counts
            _ = UpdateLastPost(maintopicid,unmoderatedposts);
            _forumservice.UpdateLastPost(mainTopic.ForumId);
            return maintopicid;
        }

        /// <summary>
        /// Locks a topic by updating its status.
        /// </summary>
        /// <remarks>This method updates the status of the specified topic in the database and saves the
        /// changes. If the topic with the given <paramref name="id"/> does not exist, no changes are made.</remarks>
        /// <param name="id">The unique identifier of the topic to be locked.</param>
        /// <param name="status">The status value to assign to the topic. Defaults to <see langword="0"/> if not specified.</param>
        /// <returns><see langword="true"/> if the topic was successfully locked; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> LockTopic(int id, short status = 0)
        {
            var topic = _dbContext.Posts.SingleOrDefault(f => f.Id == id);
            if (topic != null)
            {
                topic.Status = status;
                _dbContext.Update(topic);
            }

            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }

        /// <summary>
        /// Updates the sticky status of a post identified by its ID.
        /// </summary>
        /// <remarks>This method retrieves the post by its ID, updates its sticky status, and saves the
        /// changes to the database. If the post does not exist, no changes are made, and the method returns <see
        /// langword="false"/>.</remarks>
        /// <param name="id">The unique identifier of the post to update.</param>
        /// <param name="status">The sticky status to apply to the post. Defaults to <see langword="0"/>. A value of <see langword="0"/>
        /// typically indicates the post is not sticky, while other values may indicate sticky status.</param>
        /// <returns><see langword="true"/> if the sticky status was successfully updated; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> MakeSticky(int id, short status = 0)
        {
            var topic = _dbContext.Posts.SingleOrDefault(f => f.Id == id);
            if (topic != null)
            {
                topic.IsSticky = status;
                _dbContext.Update(topic);
            }

            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }

        /// <summary>
        /// Deletes a topic and its associated replies from the database.
        /// </summary>
        /// <remarks>If the topic exists, it is removed along with its replies, and the associated forum's
        /// last post information is updated. If the topic does not exist, no action is taken.</remarks>
        /// <param name="id">The unique identifier of the topic to delete. Must correspond to an existing topic.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task DeleteTopic(int id)
        {
            var post = _dbContext.Posts.Include(p=>p.Replies).SingleOrDefault(f => f.Id == id);
            if (post != null)
            {
                var forumid = post.ForumId;
                _dbContext.Posts.Remove(post);
                await _dbContext.SaveChangesAsync();

                _forumservice.UpdateLastPost(forumid);
                CacheProvider.Remove("AllForums");
            }
        }

        /// <summary>
        /// Deletes an archived topic and its associated replies from the database.
        /// </summary>
        /// <remarks>If the specified topic exists, it is removed along with its replies, and the
        /// associated forum's  last post information is updated. If the topic does not exist, no action is
        /// taken.</remarks>
        /// <param name="id">The unique identifier of the archived topic to delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task DeleteArchivedTopic(int id)
        {
            //
            var post = _dbContext.ArchivedTopics.Include(p=>p.ArchivedReplies).SingleOrDefault(f => f.ArchivedPostId == id);
            if (post != null)
            {
                var forumid = post.ForumId;
                _dbContext.ArchivedTopics.Remove(post);
                await _dbContext.SaveChangesAsync();

                _forumservice.UpdateLastPost(forumid);
            }
        }

        /// <summary>
        /// Deletes a reply with the specified ID from the database.
        /// </summary>
        /// <remarks>If the reply does not exist, the method completes without performing any action.
        /// Replies with a status of <see cref="Status.UnModerated"/> or <see cref="Status.OnHold"/>  may affect
        /// moderation-related data when deleted.</remarks>
        /// <param name="id">The unique identifier of the reply to delete.</param>
        /// <returns></returns>
        public async Task DeleteReply(int id)
        {
            var post = _dbContext.Replies.Find(id);
            if(post == null)
            {
                return;
            }
            int? moderated = null;
            if (post.Status == (short)Status.UnModerated || post.Status == (short)Status.OnHold)
            {
                moderated = -1;
            }
            if (post != null)
            {
                try
                {
                    var topicid = post.PostId;
                    var forumid = post.ForumId;
                    _dbContext.Replies.Remove(post);
                    _dbContext.SaveChanges();

                    await UpdateLastPost(topicid, moderated);
                    await UpdateForumLastPost(forumid);
                }
                catch (Exception e)
                {
                    _logger.Error("DeleteReply: Error fetching last topic", e);
                }


            }
        }
        private async Task UpdateForumLastPost(int forumid)
        {
            Post? lasttopic = null;
            int topiccount = 0;
            int replycount = 0;
            try
            {

                lasttopic = _dbContext.Posts.AsNoTracking().Where(t=>t.ForumId == forumid && t.Status < 2).OrderByDescending(t => t.LastPostDate).FirstOrDefault();
                topiccount = _dbContext.Posts.AsNoTracking().Count(t=>t.ForumId == forumid && t.Status <2);
                replycount = _dbContext.Replies.AsNoTracking().Count(r=>r.ForumId == forumid && r.Status <2);
            }
            catch (Exception e)
            {
                _logger.Error("UpdateForumLastPost: Error fetching last topic", e);
            }

            var forum = _dbContext.Forums.Find(forumid) ??
                new Forum
                {
                    Id = forumid
                };

            if (lasttopic == null)
            {
                forum.TopicCount = 0;
                forum.ReplyCount = 0;
                forum.LatestTopicId = 0;
                forum.LatestReplyId = 0;
                forum.LastPost = null;
                forum.LastPostAuthorId = 0;
            }
            else
            {
                forum.LatestTopicId = lasttopic.Id;
                forum.LatestReplyId = lasttopic.LastPostReplyId;
                forum.LastPost = lasttopic.LastPostDate;
                forum.LastPostAuthorId = lasttopic.LastPostAuthorId;
                forum.TopicCount = topiccount;
                forum.ReplyCount = replycount;
            }

            try
            {
                _dbContext.Forums.Attach(forum);
                _dbContext.Entry(forum).Property(x => x.TopicCount).IsModified = true;
                _dbContext.Entry(forum).Property(x => x.ReplyCount).IsModified = true;
                _dbContext.Entry(forum).Property(x => x.LatestTopicId).IsModified = true;
                _dbContext.Entry(forum).Property(x => x.LatestReplyId).IsModified = true;
                _dbContext.Entry(forum).Property(x => x.LastPost).IsModified = true;
                _dbContext.Entry(forum).Property(x => x.LastPostAuthorId).IsModified = true;

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Error("UpdateForumLastPost: Error updating last post in forum", e);

            }
            CacheProvider.Remove("AllForums");
            return;
          
        }

        /// <summary>
        /// Deletes an archived reply with the specified ID.
        /// </summary>
        /// <remarks>If the reply with the specified ID does not exist, the method completes without
        /// performing any action. Replies with certain statuses may trigger additional moderation-related
        /// updates.</remarks>
        /// <param name="id">The unique identifier of the archived reply to delete.</param>
        /// <returns></returns>
        public async Task DeleteArchivedReply(int id)
        {
            var post = _dbContext.ArchivedPosts.SingleOrDefault(f => f.Id == id);
            if(post == null)
            {
                return;
            }
            int? moderated = null;
            if (post.Status == (short)Status.UnModerated || post.Status == (short)Status.OnHold)
            {
                moderated = -1;
            }
            if (post != null)
            {
                //var topicid = post.PostId;
                //var forumid = post.ForumId;
                _dbContext.ArchivedPosts.Remove(post);
                await _dbContext.SaveChangesAsync();

                //await UpdateLastPost(topicid, moderated);
                //await _forumservice.UpdateLastPost(forumid);

            }
        }

        /// <summary>
        /// Updates the reply topic for the specified post in the database.
        /// </summary>
        /// <remarks>This method updates the specified post in the database and commits the changes
        /// asynchronously. If an error occurs during the update, the error is logged but no exception is
        /// rethrown.</remarks>
        /// <param name="post">The post object containing updated information to be saved.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task UpdateReplyTopic(Post post)
        {
            try
            {
                //var updatedTopic = _dbContext.Posts.Update(post);
                //await _dbContext.SaveChangesAsync();
                await UpdateForumLastPost(post.ForumId);
            }
            catch (Exception e)
            {
                _logger.Error("UpdateReplyTopic: Error updating post", e);
            }
            
        }


        public void Update(Post post)
        {
            if (_config.GetIntValue("STRBADWORDFILTER") == 1)
            {
                var badwords = CacheProvider.GetOrCreate("Badwords", () => _dbContext.Badwords.AsNoTracking()
                    .ToDictionary(t => t.Word), TimeSpan.FromMinutes(15));
                foreach (var badword in badwords)
                {
                    post.Content.Replace(badword.Key, badword.Value.ReplaceWith);
                }
            }

            try
            {
                if (_dbContext.Entry(post).State == EntityState.Detached)
                {
                    _dbContext.Posts.Attach(post);
                }

                _dbContext.Entry(post).Property(x => x.ForumId).IsModified = true;
                _dbContext.Entry(post).Property(x => x.CategoryId).IsModified = true;
                _dbContext.Entry(post).Property(x => x.Content).IsModified = true;
                _dbContext.Entry(post).Property(x => x.Title).IsModified = true;
                _dbContext.Entry(post).Property(x => x.IsSticky).IsModified = true;

                _dbContext.Entry(post).Property(x => x.AllowRating).IsModified = true;
                _dbContext.Entry(post).Property(x => x.Sig).IsModified = true;
                _dbContext.Entry(post).Property(x => x.Status).IsModified = true;
                _dbContext.Entry(post).Property(x => x.ArchiveFlag).IsModified = true;
                _dbContext.Entry(post).Property(x => x.LastEdit).IsModified = true;
                _dbContext.Entry(post).Property(x => x.LastEditby).IsModified = true;
                _dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.Error("UpdateTopic: Error updating post", e);
            }

        }

        public async Task Update(PostReply post)
        {
            if (_config.GetIntValue("STRBADWORDFILTER") == 1)
            {
                var badwords = CacheProvider.GetOrCreate("Badwords", () =>_dbContext.Badwords.AsNoTracking()
                    .ToDictionary(t=>t.Word),TimeSpan.FromMinutes(15));
                foreach (var badword in badwords)
                {
                    post.Content.Replace(badword.Key, badword.Value.ReplaceWith);
                }
            }

            _dbContext.Replies.Attach(post);

            _dbContext.Entry(post).Property(x => x.Content).IsModified = true;
            _dbContext.Entry(post).Property(x => x.Sig).IsModified = true;
            _dbContext.Entry(post).Property(x => x.Status).IsModified = true;
            _dbContext.Entry(post).Property(x => x.LastEdited).IsModified = true;
            _dbContext.Entry(post).Property(x => x.LastEditby).IsModified = true;

            await _dbContext.SaveChangesAsync();
        }

        public void UpdateViewCount(int id, int viewCount)
        {
            try
            {
                var itemInfoEntity = new Post()
                {
                    Id          = id,
                    ViewCount    = viewCount
                };

                _dbContext.Posts.Attach(itemInfoEntity);
                _dbContext.Entry(itemInfoEntity).Property(x => x.ViewCount).IsModified = true;
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Error("UpdateViewCount: Error updating view count", ex);
            }

        }

        public void UpdateArchivedViewCount(int id, int viewCount)
        {
            try
            {
                var itemInfoEntity = new ArchivedPost()
                {
                    ArchivedPostId          = id,
                    ViewCount    = viewCount
                };

                _dbContext.ArchivedTopics.Attach(itemInfoEntity);
                _dbContext.Entry(itemInfoEntity).Property(x => x.ViewCount).IsModified = true;
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Error("UpdateViewCount: Error updating archived view count", ex);
            }

        }

        public List<Post> GetLatestPosts(int n)
        {
                var posts = _dbContext.Posts
                .AsNoTracking()
                .Include(p => p.Forum)
                .Include(p => p.Member)
                .Include(p=>p.LastPostAuthor)
                .OrderByDescending(t=>t.LastPostDate)
                .ThenByDescending(p => p.Id)
                .AsSplitQuery();

            var member = _memberService.Current();
            var viewableForums = _memberService.ViewableForums(_user);
            if (!_user.IsInRole("Administrator") && member != null) //TODO: Is the member a moderator?
            {
                posts = posts.Where(p => (p.Status < 2 || p.MemberId == member!.Id) && viewableForums.Contains(p.ForumId));
            }else if (member == null)
            {
                posts = posts.Where(p => p.Status < 2 && p.Forum.Privateforums == ForumAuthType.All);
            }

            return posts.Take(n).ToList();
        }


        public IPagedList<PostReply> GetPagedReplies(int topicid, int pagesize = 10, int pagenumber = 1)
        {
            var replies = _dbContext.Replies.AsNoTracking().Where(p => p.PostId == topicid)
                .Include(p => p.Member).AsNoTracking()
                .OrderByDescending(post => post.Created).ThenBy(p=>p.Id)
                .Skip((pagenumber-1) * pagesize).Take(pagesize);
            return replies.ToPagedList(pagenumber, pagesize);
        }

        public IQueryable<Post> GetAllTopicsAndRelated()
        {
            return _dbContext.Posts
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Forum)
                .Include(p => p.Member)
                .Include(p=>p.LastPostAuthor)
                .OrderByDescending(t=>t.LastPostDate).ThenBy(t=>t.Id)
                .AsSplitQuery();
        }

        public async Task<Post?> GetTopicAsync(int id)
        {
            return await _dbContext.Posts
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Forum)
                .Include(p => p.Member)
                .SingleOrDefaultAsync(p => p.Id == id);

        }
        public async Task<Post?> GetTopicAsync(string title)
        {
            return await _dbContext.Posts
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Forum)
                .Include(p => p.Member)
                .SingleOrDefaultAsync(p => p.Title == title.Replace("-"," "));

        }
        public Post GetTopicForUpdate(int id)
        {
            var post = _dbContext.Posts
                //.AsNoTrackingWithIdentityResolution()
                .Single(p => p.Id == id);
            return post; 
        }

        public ArchivedPost? GetArchivedTopic(int id)
        {
            var post = _dbContext.ArchivedTopics
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Forum)
                .Include(p => p.Member)
                .SingleOrDefault(p => p.ArchivedPostId == id);
            return post;
        }

        public async Task<Post?> GetTopicWithRelated(int id)
        {

            return await _dbContext.Posts
                .AsNoTrackingWithIdentityResolution()
                .Include(p => p.Member).AsNoTracking()
                .Include(p => p.LastPostAuthor).AsNoTracking()
                .Include(p => p.Category).AsNoTracking()
                .Include(p => p.Forum).AsNoTracking()
                .Include(p => p.Replies.OrderByDescending(r => r.Created))
                .ThenInclude(r => r.Member).AsNoTracking()
                //.AsSplitQuery()
                .SingleOrDefaultAsync(p => p.Id == id);

        }

        public ArchivedPost? GetArchivedTopicWithRelated(int id)
        {

            var post = _dbContext.ArchivedTopics.Where(p => p.ArchivedPostId == id)
                .AsNoTracking()
                .Include(p => p.Member).AsNoTracking()
                .Include(p => p.LastPostAuthor).AsNoTracking()
                .Include(p => p.Category).AsNoTracking()
                .Include(p => p.Forum).AsNoTracking()
                .Include(p => p.ArchivedReplies.OrderByDescending(r => r.Created))
                .ThenInclude(r => r.Member).AsNoTracking()
                //.AsSplitQuery()
                .Single();

            return post;
        }

        public PostReply GetReply(int id)
        {

            var post = _dbContext.Replies.Where(p => p.Id == id)
                .AsNoTrackingWithIdentityResolution()
                .Include(p => p.Member).AsNoTrackingWithIdentityResolution()
                .Include(r => r.Topic).ThenInclude(t=>t!.Member).AsNoTracking()
                .Single();

            return post;
        }

        public ArchivedReply GetArchivedReply(int id)
        {

            var post = _dbContext.ArchivedPosts.Where(p => p.Id == id)
                .AsNoTrackingWithIdentityResolution()
                .Include(p => p.Member).AsNoTrackingWithIdentityResolution()
                .Include(r => r.Topic).ThenInclude(t=>t!.Member).AsNoTracking()
                .Single();

            return post;
        }

        public PostReply GetReplyForUdate(int id)
        {

            var post = _dbContext.Replies.Where(p => p.Id == id)
                .AsNoTracking()
                .Single();

            return post;
        }

        public IPagedList<Post> GetFilteredPost(string? searchQuery,out int totalcount, int pagesize=25, int page=1,int catid=0,int forumid=0)
        {
            if (searchQuery == null)
            {
                totalcount = 0;
                return new PagedList<Post>(Array.Empty<Post>(), 1, pagesize);
            }
            var posts = _dbContext.Posts.AsNoTracking().Where(p=>p.Title.Contains(searchQuery) || p.Content.Contains(searchQuery));

            posts = posts.Include(p => p.Forum).OrderByDescending(p=>p.LastPostDate??p.Created);
            if (catid > 0)
            {
                posts = posts.Where(p => p.CategoryId == catid);
            }
            if (forumid > 0)
            {
                posts = posts.Where(p => p.ForumId == forumid);
            }

            totalcount = posts.Count();
            return posts.ToPagedList(page, pagesize);
        }

        public IPagedList<Post> Find(ForumSearch searchQuery, out int totalcount, int pagesize, int page)
        {

            var posts = _dbContext.Posts                
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Forum)
                .Include(p => p.Member)
                .Include(p=>p.LastPostAuthor)
                .Include(p=>p.Replies!).ThenInclude(r=>r.Member)
                .AsQueryable();

            if (searchQuery.SinceDate != SearchDate.AnyDate)
            {
                var lastvisit = DateTime.UtcNow.AddDays(-(int)searchQuery.SinceDate).ToForumDateStr();
                posts = posts
                    
                .Where(f => string.Compare( f.LastPostDate , lastvisit) > 0)
                .OrderByDescending(t=>t.LastPostDate).AsQueryable();
            }
            if (searchQuery.SearchCategory is > 0)
            {
                posts = posts.Where(p => p.CategoryId == searchQuery.SearchCategory);
            }
            if (searchQuery.SearchForums != null && searchQuery.SearchForums.Any())
            {
                posts = posts.Where(p => searchQuery.SearchForums.Contains(p.ForumId));
            }
            if (!string.IsNullOrWhiteSpace(searchQuery.UserName) && searchQuery.SearchMessage)
            {
                var p1 = posts.Where(p=> p.Replies.Any(r=>r.Member!.Name.ToLower().StartsWith(searchQuery.UserName.ToLower()))).ToList();

                var p2 = posts.Where(p => p.Member!.Name.ToLower().StartsWith(searchQuery.UserName.ToLower())).ToList();

                posts = p1.UnionBy(p2, x => x.Id).AsQueryable();
            }
            if (!string.IsNullOrWhiteSpace(searchQuery.UserName) && !searchQuery.SearchMessage)
            {
                posts = posts.Where(p => p.Member!.Name.ToLower().StartsWith(searchQuery.UserName.ToLower()));

            }
            if (searchQuery.Terms != null)
            {
                var terms = searchQuery.Terms.Split(' ');
                switch (searchQuery.SearchFor)
                {
                    case SearchFor.AllTerms:
                        posts = searchQuery.SearchMessage ? posts
                            .Where(p => p.Title.ContainsAll(terms) || p.Content.ContainsAll(terms)) : posts.Where(p => p.Title.ContainsAll(terms));
                        break;
                    case SearchFor.AnyTerms :
                        posts = searchQuery.SearchMessage ? posts
                            .Where(p => p.Title.ContainsAny(terms) || p.Content.ContainsAny(terms)) : posts.Where(p => p.Title.ContainsAny(terms));
                        break;
                    case SearchFor.ExactPhrase :
                        posts = searchQuery.SearchMessage ? posts.Where(p => p.Title.Contains(searchQuery.Terms) || p.Content.Contains(searchQuery.Terms)) : posts.Where(p => p.Title.Contains(searchQuery.Terms));
                        break;
                }
            }


            totalcount = posts.Count();
            return posts.OrderByDescending(p=>p.LastPostDate).ToPagedList(page, pagesize);
        }

        public IPagedList<ArchivedPost> FindArchived(ForumSearch searchQuery, out int totalcount, int pagesize, int page)
        {

            var posts = _dbContext.ArchivedTopics                
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Forum)
                .Include(p => p.Member)
                .Include(p=>p.LastPostAuthor)
                //.Include(p=>p.ArchivedReplies!).ThenInclude(r=>r.Member)
                .AsQueryable();

            if (searchQuery.SinceDate != SearchDate.AnyDate)
            {
                var lastvisit = DateTime.UtcNow.AddDays(-(int)searchQuery.SinceDate).ToForumDateStr();
                posts = posts
                    
                .Where(f => string.Compare( f.LastPostDate , lastvisit) > 0)
                .OrderByDescending(t=>t.LastPostDate).AsQueryable();
            }
            if (searchQuery.SearchCategory is > 0)
            {
                posts = posts.Where(p => p.CategoryId == searchQuery.SearchCategory);
            }
            if (searchQuery.SearchForums != null && searchQuery.SearchForums.Any())
            {
                posts = posts.Where(p => searchQuery.SearchForums.Contains(p.ForumId));
            }
            if (!string.IsNullOrWhiteSpace(searchQuery.UserName) && searchQuery.SearchMessage)
            {
                var p1 = posts.Where(p=> p.ArchivedReplies.Any(r=>r.Member!.Name.ToLower().StartsWith(searchQuery.UserName.ToLower()))).ToList();

                var p2 = posts.Where(p => p.Member!.Name.ToLower().StartsWith(searchQuery.UserName.ToLower())).ToList();

                posts = p1.UnionBy(p2, x => x.ArchivedPostId).AsQueryable();
            }
            if (!string.IsNullOrWhiteSpace(searchQuery.UserName) && !searchQuery.SearchMessage)
            {
                posts = posts.Where(p => p.Member!.Name.ToLower().StartsWith(searchQuery.UserName.ToLower()));

            }
            if (searchQuery.Terms != null)
            {
                var terms = searchQuery.Terms.Split(' ');
                switch (searchQuery.SearchFor)
                {
                    case SearchFor.AllTerms:
                        posts = searchQuery.SearchMessage ? posts
                            .Where(p => p.Subject.ContainsAll(terms) || p.Message.ContainsAll(terms)) : posts.Where(p => p.Subject.ContainsAll(terms));
                        break;
                    case SearchFor.AnyTerms :
                        posts = searchQuery.SearchMessage ? posts
                            .Where(p => p.Subject.ContainsAny(terms) || p.Message.ContainsAny(terms)) : posts.Where(p => p.Subject.ContainsAny(terms));
                        break;
                    case SearchFor.ExactPhrase :
                        posts = searchQuery.SearchMessage ? posts.Where(p => p.Subject.Contains(searchQuery.Terms) || p.Message.Contains(searchQuery.Terms)) : posts.Where(p => p.Subject.Contains(searchQuery.Terms));
                        break;
                }
            }


            totalcount = posts.Count();
            return posts.OrderByDescending(p=>p.LastPostDate).ToPagedList(page, pagesize);
        }

        public Post GetLatestReply(int id)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateLastPost(int topicid, int? moderatedcount, bool wasdraft = false)
        {
            var count = _dbContext.Replies.Count(r => r.PostId == topicid && r.Status < 2);
            var lastreply = _dbContext.Replies.AsNoTracking()
                .Where(t=>t.PostId == topicid && t.Status < 2)
                .OrderByDescending(t=>t.Created)
                .Select(p => new { LastPostId = p.Id, LastPostAuthorId = p.MemberId,LastPostDate = p.Created })
                .FirstOrDefault();

            var topic = _dbContext.Posts.Find(topicid);// GetTopicForUpdate(topicid);
            if(topic == null)
            {
                _logger.Error($"UpdateLastPost: Topic with ID {topicid} not found.");
                return ;
            }

            if (lastreply == null)
            {
                topic.LastPostReplyId = 0;
                topic.LastPostDate = topic.Created;
                topic.LastPostAuthorId = topic.MemberId;
                topic.ReplyCount = 0;
            }
            else
            {
                topic.LastPostReplyId = lastreply.LastPostId;
                topic.LastPostDate = lastreply.LastPostDate;
                topic.LastPostAuthorId = lastreply.LastPostAuthorId;
                topic.ReplyCount = count;
            }

            if (moderatedcount != null)
            {
                topic.UnmoderatedReplies += moderatedcount.Value;
            }
            try
            {
                _dbContext.Update(topic);
                _dbContext.SaveChanges();
                if (wasdraft)
                {
                        var forum = _forumservice.UpdateLastPost(topic.ForumId);
                        if (forum.CountMemberPosts == 1)
                        {
                            await _memberService.UpdatePostCount(topic.MemberId);
                        }
                        else
                        {
                            await _memberService.UpdateLastPost(topic.MemberId);
                        }
            
                        var forumtotals = _dbContext.ForumTotal.OrderBy(t=>t.Id).First();
                        forumtotals.TopicCount += 1;
                        CacheProvider.Remove("AllForums");
                        _dbContext.SaveChanges();
                }
            }
            catch (Exception e)
            {
                _logger.Error($"UpdateLastPost:", e);
            }


        }

        public async Task<bool> Answer(int id)
        {
            var reply = _dbContext.Replies.OrderBy(m=>m.Id).FirstOrDefault(r => r.Id == id);
            if (reply != null)
            {
                var topic = _dbContext.Posts.OrderBy(m=>m.Id).First(t => t.Id == reply.PostId);
                reply.Answer = true;
                topic.Answered = true;
                _dbContext.Update(reply);
                _dbContext.Update(topic);
                await _dbContext.SaveChangesAsync();
            }

            return true;
        }

        public async Task SetStatus(int id, Status status)
        {
            var topic = _dbContext.Posts.Find(id);
            if (topic != null)
            {
                topic.Status = (short)status;
                _dbContext.Update(topic);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task SetReplyStatus(int id, Status status)
        {
            var reply = _dbContext.Replies.Find(id);
            if (reply != null)
            {
                reply.Status = (short)status;
                _dbContext.Update(reply);
                await _dbContext.SaveChangesAsync();

            }
        }

        public Poll? GetPoll(int topicid)
        {
            return _dbContext.Polls.AsNoTracking().Include(p=>p.PollAnswers).SingleOrDefault(p=>p.TopicId ==topicid);
        }

        public List<Post> GetById(int[] ids)
        {
            return _dbContext.Posts.AsNoTracking().Where(p => EF.Constant(ids).Contains(p.Id)).OrderBy(t => t.Created).ToList();
        }

        public void MoveSubscriptions(int oldtopicid, int newtopicid, int newforumId, int newcatId)
        {
            _dbContext.Database.ExecuteSql($"UPDATE {_tableprefix}SUBSCRIPTIONS SET TOPIC_ID={newtopicid}, FORUM_ID={newforumId}, CAT_ID={newcatId} WHERE TOPIC_ID={oldtopicid}");
        }

        public void MoveReplies(int oldtopicid, Post newTopic)
        {
            _dbContext.Database.ExecuteSql(
                $"UPDATE {_tableprefix}REPLY SET TOPIC_ID={newTopic.Id},FORUM_ID={newTopic.ForumId},CAT_ID={newTopic.CategoryId} WHERE TOPIC_ID={oldtopicid}");

        }

        public Post? SplitTopic(string[] ids, int forumId, string subject)
        {
            var forum = _dbContext.Forums.AsNoTracking().FirstOrDefault(f=>f.Id == forumId);

            int replycount = 0;
            int originaltopicid = 0;
            Post? topic = null;
            bool first = true;
            try
            {
                _dbContext.Database.BeginTransaction();
                foreach (string id in ids.OrderBy(s => s))
                {
                    //fetch the reply
                    var reply = _dbContext.Replies.SingleOrDefault(r=>r.Id == Convert.ToInt32(id));

                    if (first && reply != null)
                    {
                        originaltopicid = reply.PostId;
                        //first reply so create the Topic
                        topic = new Post
                        {
                            CategoryId = forum.CategoryId,
                            ForumId = forum.Id,
                            Title = subject,
                            Content = reply.Content,
                            Created = reply.Created,
                            MemberId = reply.MemberId,
                            Sig = reply.Sig,
                            LastPostAuthorId = reply.MemberId,
                            LastPostReplyId = reply.Id,
                            Status =(short)Status.Open,
                            LastPostDate = reply.Created,
                            ReplyCount = 0,
                            UnmoderatedReplies = 0
                        };

                        _dbContext.Add(topic);
                        _dbContext.Remove(reply);
                        _dbContext.SaveChanges();
                        first = false;
                    }
                    else if (topic != null && reply != null)
                    {
                        replycount += 1;

                        reply.PostId = topic.Id;
                        reply.ForumId = forum.Id;
                        reply.CategoryId = forum.CategoryId;
                        reply.Status = topic.Status;
                        
                        _dbContext.Update(reply);
                        _dbContext.SaveChanges();
                    }
                }
                if(topic != null)
                {

                    topic.ReplyCount = replycount;
                    _dbContext.Update(topic);
                    _dbContext.SaveChanges();
                }
            }
            catch (Exception e)
            {
                _logger.Error("SplitTopic: Error splitting topic, rolling back",e);
                _dbContext.Database.RollbackTransaction();
            }        
            finally
            {
                _dbContext.Database.CommitTransaction();
            }
            if(topic != null)
            {
                _ = UpdateLastPost(topic.Id, null);
                _forumservice.UpdateLastPost(topic.ForumId);
            }


            Post? originaltopic = (from t in _dbContext.Posts select t).OrderBy(m=>m.Id).FirstOrDefault(t=>t.Id == originaltopicid);
            if (originaltopic != null)
            {
                _ = UpdateLastPost(originaltopicid, 0);
                _forumservice.UpdateLastPost(originaltopic.ForumId);
            }
            return topic;
        }

        public decimal GetTopicRating(int topicid)
        {
            var ratings = _dbContext.Posts
                .AsNoTracking()
                .Single(p => p.Id == topicid);

            decimal rating = 0;
            if (ratings.RatingTotal > 0)
            {
                decimal ratingSum = Decimal.Divide((decimal)ratings.RatingTotal, 10);
                var ratingCount = (decimal)ratings.RatingTotalCount;
                rating = (ratingSum / ratingCount);
            }
            return decimal.Parse(rating.ToString());
        }

        public decimal GetReplyRating(int topicid)
        {
            var ratings = _dbContext.Replies
                .AsNoTracking()
                .Where(r=>r.PostId == topicid && r.Status<2)
                .ToList();

            decimal rating = 0;
            if (ratings.Any())
            {
                decimal ratingSum = Decimal.Divide(ratings.Sum(d => d.Rating),10);
                var ratingCount = ratings.Count;
                rating = (ratingSum / ratingCount);
            }
            return decimal.Parse(rating.ToString());
        }

        public Post? GetTopic(int id)
        {
            return _dbContext.Posts.AsNoTracking().FirstOrDefault(p => p.Id == id);

        }
    }
}
