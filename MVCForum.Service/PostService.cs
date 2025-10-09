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

        /// <summary>
        /// Updates the specified post in the database, applying any necessary modifications and saving the changes.
        /// </summary>
        /// <remarks>If the bad word filter is enabled in the configuration, the method replaces any
        /// occurrences of bad words in the post content  with their corresponding replacement values. The bad word list
        /// is cached for performance optimization.  The method ensures that the post entity is properly tracked by the
        /// database context before marking its properties as modified.  Only specific properties of the post are
        /// updated, including its content, title, and metadata such as status and last edit details.  Any errors
        /// encountered during the update process are logged for diagnostic purposes.</remarks>
        /// <param name="post">The post to be updated. The post must not be null and should contain valid data for all required fields.</param>
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

        /// <summary>
        /// Updates the specified post reply in the database, applying any configured content filters.
        /// </summary>
        /// <remarks>If the bad word filter is enabled in the configuration, the method replaces any
        /// occurrences of bad words in the post content with their configured replacements before updating the
        /// database.</remarks>
        /// <param name="post">The <see cref="PostReply"/> object to update. The object must already exist in the database.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
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

        /// <summary>
        /// Updates the view count for a specific post.
        /// </summary>
        /// <remarks>This method updates the view count of a post in the database. If the specified post
        /// does not exist,  no changes will be made. Ensure that the <paramref name="id"/> corresponds to a valid
        /// post.</remarks>
        /// <param name="id">The unique identifier of the post to update.</param>
        /// <param name="viewCount">The new view count value to set for the post.</param>
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

        /// <summary>
        /// Updates the view count for an archived post with the specified identifier.
        /// </summary>
        /// <remarks>This method updates the view count of an archived post in the database.  If the
        /// specified post does not exist, no changes will be made.</remarks>
        /// <param name="id">The unique identifier of the archived post to update.</param>
        /// <param name="viewCount">The new view count to set for the archived post.</param>
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

        /// <summary>
        /// Retrieves the latest posts from the database, ordered by the most recent post date.
        /// </summary>
        /// <remarks>This method applies filtering based on the current user's permissions and roles.  -
        /// If the user is an administrator, all posts are included. - If the user is a regular member, only posts in
        /// viewable forums or authored by the member are included. - If the user is not authenticated, only public
        /// posts in forums accessible to all users are included.</remarks>
        /// <param name="n">The maximum number of posts to retrieve. Must be a positive integer.</param>
        /// <returns>A list of the latest posts, including their associated forum, author, and last post author details.  The
        /// list may be empty if no posts are available or accessible to the current user.</returns>
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

        /// <summary>
        /// Retrieves a paginated list of replies for a specified topic.
        /// </summary>
        /// <remarks>Replies are ordered by their creation date in descending order, and then by their
        /// unique identifier  in ascending order. The method uses no-tracking queries to improve performance when data
        /// tracking  is not required.</remarks>
        /// <param name="topicid">The unique identifier of the topic for which replies are retrieved.</param>
        /// <param name="pagesize">The number of replies to include on each page. The default value is 10.</param>
        /// <param name="pagenumber">The page number to retrieve. The default value is 1.</param>
        /// <returns>A paginated list of <see cref="PostReply"/> objects representing the replies for the specified topic.</returns>
        public IPagedList<PostReply> GetPagedReplies(int topicid, int pagesize = 10, int pagenumber = 1)
        {
            var replies = _dbContext.Replies.AsNoTracking().Where(p => p.PostId == topicid)
                .Include(p => p.Member).AsNoTracking()
                .OrderByDescending(post => post.Created).ThenBy(p=>p.Id)
                .Skip((pagenumber-1) * pagesize).Take(pagesize);
            return replies.ToPagedList(pagenumber, pagesize);
        }

        /// <summary>
        /// Retrieves all topics along with their related entities, including category, forum, member, and last post
        /// author.
        /// </summary>
        /// <remarks>The returned query is configured to not track changes to the entities and includes
        /// related data for the  specified navigation properties. The results are ordered by the last post date in
        /// descending order,  followed by the topic ID in ascending order.</remarks>
        /// <returns>An <see cref="IQueryable{T}"/> of <see cref="Post"/> representing the topics and their related data.</returns>
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

        /// <summary>
        /// Retrieves a forum post by its unique identifier.
        /// </summary>
        /// <remarks>The returned <see cref="Post"/> object includes related data for the post's category,
        /// forum,  and member, as these relationships are eagerly loaded.</remarks>
        /// <param name="id">The unique identifier of the forum post to retrieve.</param>
        /// <returns>A <see cref="Post"/> object representing the forum post with the specified identifier,  or <see
        /// langword="null"/> if no post with the given identifier exists.</returns>
        public async Task<Post?> GetTopicAsync(int id)
        {
            return await _dbContext.Posts
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Forum)
                .Include(p => p.Member)
                .SingleOrDefaultAsync(p => p.Id == id);

        }

        /// <summary>
        /// Retrieves a forum post based on the specified title.
        /// </summary>
        /// <remarks>The returned post includes related data for its category, forum, and member, as these
        /// are eagerly loaded. The operation is performed without tracking changes to the retrieved entity.</remarks>
        /// <param name="title">The title of the post to retrieve. Hyphens in the title will be replaced with spaces during the search.</param>
        /// <returns>The <see cref="Post"/> object that matches the specified title, or <see langword="null"/> if no matching
        /// post is found.</returns>
        public async Task<Post?> GetTopicAsync(string title)
        {
            return await _dbContext.Posts
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Forum)
                .Include(p => p.Member)
                .SingleOrDefaultAsync(p => p.Title == title.Replace("-"," "));

        }

        /// <summary>
        /// Retrieves a post by its unique identifier for the purpose of updating it.
        /// </summary>
        /// <param name="id">The unique identifier of the post to retrieve.</param>
        /// <returns>The <see cref="Post"/> object with the specified identifier.</returns>
        public Post GetTopicForUpdate(int id)
        {
            var post = _dbContext.Posts
                //.AsNoTrackingWithIdentityResolution()
                .Single(p => p.Id == id);
            return post; 
        }

        /// <summary>
        /// Retrieves an archived topic by its unique identifier.
        /// </summary>
        /// <remarks>This method performs a database query to retrieve the archived topic, including its
        /// associated  category, forum, and member information. The query is executed with no tracking to improve
        /// performance  for read-only operations.</remarks>
        /// <param name="id">The unique identifier of the archived topic to retrieve.</param>
        /// <returns>An <see cref="ArchivedPost"/> object representing the archived topic if found; otherwise, <see
        /// langword="null"/>.</returns>
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

        /// <summary>
        /// Retrieves a topic along with its related entities based on the specified topic ID.
        /// </summary>
        /// <remarks>The method includes related entities such as the topic's member, last post author,
        /// category, forum, and replies (ordered by creation date in descending order). The data is retrieved using
        /// no-tracking queries to ensure the entities are not tracked by the context.</remarks>
        /// <param name="id">The unique identifier of the topic to retrieve.</param>
        /// <returns>A <see cref="Post"/> object representing the topic and its related entities, or <see langword="null"/> if no
        /// topic with the specified ID exists.</returns>
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

        /// <summary>
        /// Retrieves an archived topic along with its related data based on the specified identifier.
        /// </summary>
        /// <remarks>The related data includes the topic's member, last post author, category, forum, and
        /// replies,  with replies ordered by creation date in descending order. All data is retrieved without tracking 
        /// changes in the database context.</remarks>
        /// <param name="id">The unique identifier of the archived topic to retrieve.</param>
        /// <returns>An <see cref="ArchivedPost"/> object containing the archived topic and its related data,  or <see
        /// langword="null"/> if no topic with the specified identifier exists.</returns>
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

        /// <summary>
        /// Retrieves a reply by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the reply to retrieve.</param>
        /// <returns>A <see cref="PostReply"/> object representing the reply with the specified identifier.</returns>
        public PostReply GetReply(int id)
        {

            var post = _dbContext.Replies.Where(p => p.Id == id)
                .AsNoTrackingWithIdentityResolution()
                .Include(p => p.Member).AsNoTrackingWithIdentityResolution()
                .Include(r => r.Topic).ThenInclude(t=>t!.Member).AsNoTracking()
                .Single();

            return post;
        }

        /// <summary>
        /// Retrieves an archived reply by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the archived reply to retrieve.</param>
        /// <returns>An <see cref="ArchivedReply"/> object representing the archived reply with the specified identifier.</returns>
        public ArchivedReply GetArchivedReply(int id)
        {

            var post = _dbContext.ArchivedPosts.Where(p => p.Id == id)
                .AsNoTrackingWithIdentityResolution()
                .Include(p => p.Member).AsNoTrackingWithIdentityResolution()
                .Include(r => r.Topic).ThenInclude(t=>t!.Member).AsNoTracking()
                .Single();

            return post;
        }

        /// <summary>
        /// Retrieves a reply associated with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the reply to retrieve.</param>
        /// <returns>The <see cref="PostReply"/> object corresponding to the specified identifier.</returns>
        public PostReply GetReplyForUdate(int id)
        {

            var post = _dbContext.Replies.Where(p => p.Id == id)
                .AsNoTracking()
                .Single();

            return post;
        }

        /// <summary>
        /// Retrieves a paginated list of posts filtered by the specified search query, category, and forum.
        /// </summary>
        /// <remarks>The method performs a case-insensitive search on the title and content of posts.
        /// Results are ordered by the  most recent activity, using the last post date or creation date if the last post
        /// date is unavailable.</remarks>
        /// <param name="searchQuery">The search query to filter posts by their title or content. If <see langword="null"/>, an empty list is
        /// returned.</param>
        /// <param name="totalcount">When the method returns, contains the total number of posts matching the filter criteria.</param>
        /// <param name="pagesize">The number of posts to include in each page. The default value is 25.</param>
        /// <param name="page">The page number to retrieve. The default value is 1.</param>
        /// <param name="catid">The ID of the category to filter posts by. Use 0 to include all categories. The default value is 0.</param>
        /// <param name="forumid">The ID of the forum to filter posts by. Use 0 to include all forums. The default value is 0.</param>
        /// <returns>A paginated list of posts matching the specified filter criteria. If no posts match, an empty list is
        /// returned.</returns>
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

        /// <summary>
        /// Searches for forum posts based on the specified search criteria and returns a paginated list of results.
        /// </summary>
        /// <remarks>The search criteria can include filtering by date, category, forums, user name, and
        /// specific search terms. The results are ordered by the most recent post date. The method supports searching
        /// within post titles and content, as well as filtering by user name and replies.</remarks>
        /// <param name="searchQuery">The search criteria used to filter the forum posts. This includes options such as date range, category,
        /// forums, user name, and search terms.</param>
        /// <param name="totalcount">When this method returns, contains the total number of posts matching the search criteria.</param>
        /// <param name="pagesize">The number of posts to include in each page of the results. Must be greater than zero.</param>
        /// <param name="page">The page number to retrieve. Must be greater than zero.</param>
        /// <returns>A paginated list of forum posts that match the specified search criteria. If no posts match, the list will
        /// be empty.</returns>
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

        /// <summary>
        /// Retrieves a paginated list of archived forum posts based on the specified search criteria.
        /// </summary>
        /// <remarks>This method supports filtering by various criteria, including date range, category,
        /// forums, user name,  and search terms. The results are ordered by the date of the last post in descending
        /// order.</remarks>
        /// <param name="searchQuery">The search criteria used to filter the archived posts.</param>
        /// <param name="totalcount">When this method returns, contains the total number of posts that match the search criteria.</param>
        /// <param name="pagesize">The number of posts to include in each page of the result set. Must be greater than zero.</param>
        /// <param name="page">The page number to retrieve. Must be greater than zero.</param>
        /// <returns>A paginated list of <see cref="ArchivedPost"/> objects that match the specified search criteria. If no posts
        /// match the criteria, the returned list will be empty.</returns>
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

        /// <summary>
        /// Retrieves the most recent reply to a post with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the post for which to retrieve the latest reply.</param>
        /// <returns>The latest <see cref="Post"/> object representing the reply, or <see langword="null"/> if no replies are
        /// found.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public Post GetLatestReply(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the metadata of the specified topic, including the last post information, reply count,  and
        /// unmoderated reply count. Optionally updates related forum and member data if the topic was a draft.
        /// </summary>
        /// <remarks>This method updates the topic's last post information based on the most recent reply.
        /// If no replies exist, the topic's creation details are used instead.  When <paramref name="wasdraft"/> is
        /// <see langword="true"/>, the method also updates the forum's last post  and the member's post count or last
        /// post information.</remarks>
        /// <param name="topicid">The unique identifier of the topic to update.</param>
        /// <param name="moderatedcount">The number of moderated replies to add to the topic's unmoderated reply count.  If <see langword="null"/>,
        /// no changes are made to the unmoderated reply count.</param>
        /// <param name="wasdraft">A value indicating whether the topic was previously a draft.  If <see langword="true"/>, additional updates
        /// are performed for the forum and member associated with the topic.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Marks the specified reply as the answer to its associated topic and updates the topic's status accordingly.
        /// </summary>
        /// <remarks>This method updates the reply and its associated topic in the database to reflect
        /// that the reply is the accepted answer. If the reply with the specified <paramref name="id"/> does not exist,
        /// no changes are made.</remarks>
        /// <param name="id">The unique identifier of the reply to be marked as the answer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the
        /// operation completes successfully.</returns>
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

        /// <summary>
        /// Updates the status of a post with the specified identifier.
        /// </summary>
        /// <remarks>If the post with the specified <paramref name="id"/> does not exist, no changes are
        /// made.</remarks>
        /// <param name="id">The unique identifier of the post to update.</param>
        /// <param name="status">The new status to assign to the post.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Retrieves the poll associated with the specified topic ID.
        /// </summary>
        /// <remarks>The returned poll is retrieved without tracking changes in the database context. This
        /// ensures that the  entity is read-only and will not be updated in the database unless explicitly attached and
        /// modified.</remarks>
        /// <param name="topicid">The unique identifier of the topic for which the poll is being retrieved.</param>
        /// <returns>The <see cref="Poll"/> object associated with the specified topic ID, including its related poll answers, 
        /// or <see langword="null"/> if no poll exists for the given topic ID.</returns>
        public Poll? GetPoll(int topicid)
        {
            return _dbContext.Polls.AsNoTracking().Include(p=>p.PollAnswers).SingleOrDefault(p=>p.TopicId ==topicid);
        }

        /// <summary>
        /// Retrieves a list of posts that match the specified identifiers.
        /// </summary>
        /// <param name="ids">An array of post identifiers to retrieve. Cannot be null.</param>
        /// <returns>A list of <see cref="Post"/> objects that match the specified identifiers. The list will be empty if no
        /// matching posts are found.</returns>
        public List<Post> GetById(int[] ids)
        {
            return _dbContext.Posts.AsNoTracking().Where(p => EF.Constant(ids).Contains(p.Id)).OrderBy(t => t.Created).ToList();
        }

        /// <summary>
        /// Moves subscriptions from one topic to another, updating the associated forum and category identifiers.
        /// </summary>
        /// <remarks>This method updates the database to reassign subscriptions from the specified old
        /// topic to the new topic. Ensure that the provided topic, forum, and category identifiers are valid and exist
        /// in the database.</remarks>
        /// <param name="oldtopicid">The identifier of the topic from which subscriptions will be moved.</param>
        /// <param name="newtopicid">The identifier of the topic to which subscriptions will be moved.</param>
        /// <param name="newforumId">The identifier of the forum associated with the new topic.</param>
        /// <param name="newcatId">The identifier of the category associated with the new topic.</param>
        public void MoveSubscriptions(int oldtopicid, int newtopicid, int newforumId, int newcatId)
        {
            _dbContext.Database.ExecuteSql($"UPDATE {_tableprefix}SUBSCRIPTIONS SET TOPIC_ID={newtopicid}, FORUM_ID={newforumId}, CAT_ID={newcatId} WHERE TOPIC_ID={oldtopicid}");
        }

        /// <summary>
        /// Moves all replies from one topic to another.
        /// </summary>
        /// <remarks>This method updates the database to reassign replies associated with the specified
        /// topic ID  to the new topic. Ensure that <paramref name="newTopic"/> represents a valid and existing topic 
        /// before calling this method.</remarks>
        /// <param name="oldtopicid">The identifier of the topic from which replies will be moved.</param>
        /// <param name="newTopic">The new topic to which the replies will be reassigned. Must contain valid topic, forum, and category
        /// identifiers.</param>
        public void MoveReplies(int oldtopicid, Post newTopic)
        {
            _dbContext.Database.ExecuteSql(
                $"UPDATE {_tableprefix}REPLY SET TOPIC_ID={newTopic.Id},FORUM_ID={newTopic.ForumId},CAT_ID={newTopic.CategoryId} WHERE TOPIC_ID={oldtopicid}");

        }

        /// <summary>
        /// Splits a topic by creating a new topic from the specified replies and moving them to the new topic.
        /// </summary>
        /// <remarks>This method creates a new topic in the specified forum using the content of the first
        /// reply in the provided list of reply IDs. Subsequent replies are moved to the new topic. The original topic's
        /// metadata is updated to reflect the removal of the replies.</remarks>
        /// <param name="ids">An array of reply IDs to be moved to the new topic. The IDs must correspond to valid replies in the
        /// database.</param>
        /// <param name="forumId">The ID of the forum where the new topic will be created. The forum must exist.</param>
        /// <param name="subject">The subject of the new topic. This will be used as the title of the new topic.</param>
        /// <returns>The newly created <see cref="Post"/> representing the new topic, or <see langword="null"/> if the operation
        /// fails.</returns>
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

        /// <summary>
        /// Calculates the average rating for a specified topic.
        /// </summary>
        /// <remarks>The rating is calculated as the total rating divided by the number of ratings, scaled
        /// by a factor of 10.</remarks>
        /// <param name="topicid">The unique identifier of the topic for which the rating is to be retrieved.</param>
        /// <returns>The average rating of the topic as a <see cref="decimal"/>.  Returns 0 if the topic has no ratings.</returns>
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

        /// <summary>
        /// Calculates the average rating of replies for a given topic.
        /// </summary>
        /// <remarks>Only replies with a status less than 2 are included in the calculation. The rating is
        /// normalized by dividing the sum of reply ratings by 10 before averaging.</remarks>
        /// <param name="topicid">The unique identifier of the topic for which to calculate the reply rating.</param>
        /// <returns>The average rating of replies for the specified topic. Returns 0 if there are no replies or if all replies
        /// are excluded based on their status.</returns>
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

        /// <summary>
        /// Retrieves a topic by its unique identifier.
        /// </summary>
        /// <remarks>The returned <see cref="Post"/> object is retrieved without tracking changes in the
        /// database context.</remarks>
        /// <param name="id">The unique identifier of the topic to retrieve.</param>
        /// <returns>The <see cref="Post"/> object representing the topic if found; otherwise, <see langword="null"/>.</returns>
        public Post? GetTopic(int id)
        {
            return _dbContext.Posts.AsNoTracking().FirstOrDefault(p => p.Id == id);

        }
    }
}
