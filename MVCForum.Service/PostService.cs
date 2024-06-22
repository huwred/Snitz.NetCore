using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace SnitzCore.Service
{
    public class PostService : IPost
    {
        private readonly SnitzDbContext _dbContext;
        private readonly IMember _memberService;
        private readonly IForum _forumservice;
        private readonly ISnitzConfig _config;
        private readonly IEmailSender _mailSender;
        private readonly string _tableprefix;
        public PostService(SnitzDbContext dbContext, IMember memberService,IForum forumservice,ISnitzConfig config,IEmailSender mailSender,IOptions<SnitzForums> options)
        {
            _dbContext = dbContext;
            _memberService = memberService;
            _forumservice = forumservice;
            _config = config;
            _mailSender = mailSender;
            _tableprefix = options.Value.forumTablePrefix;
        }

        /// <summary>
        /// Create a Topic
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public async Task<int> Create(Post post)
        {
            if (_config.GetIntValue("STRBADWORDFILTER") == 1)
            {
                var badwords = _dbContext.Badwords.AsNoTracking()
                    .ToDictionary(t=>t.Word);
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

            var forum = await _forumservice.UpdateLastPost(post.ForumId);
            
            if (forum.CountMemberPosts == 1)
            {
                await _memberService.UpdatePostCount(post.MemberId);
            }
            else
            {
                _memberService.UpdateLastPost(post.MemberId);
            }
            
            var forumtotals = _dbContext.ForumTotal.First();
            forumtotals.TopicCount += 1;
            await _dbContext.SaveChangesAsync();
            return post.Id;
        }

        /// <summary>
        /// Create a Reply
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public async Task<int> Create(PostReply post)
        {
            if (_config.GetIntValue("STRBADWORDFILTER") == 1)
            {
                var badwords = _dbContext.Badwords.AsNoTracking()
                    .ToDictionary(t=>t.Word);
                foreach (var badword in badwords)
                {
                    post.Content.Replace(badword.Key, badword.Value.ReplaceWith);
                }
            }
            _dbContext.Replies.Add(post);
            await _dbContext.SaveChangesAsync();
            int? moderated = null;
            //update topic stuff
            if (post.Status == (short)Status.UnModerated)
            {
                moderated = 1;
            }
            await UpdateLastPost(post.PostId,moderated);
            //update Forum
            var forum = await _forumservice.UpdateLastPost(post.ForumId);
            if (forum.CountMemberPosts == 1)
            {
                await _memberService.UpdatePostCount(post.MemberId);
            }else
            {
                _memberService.UpdateLastPost(post.MemberId);
            }
            var forumtotals = _dbContext.ForumTotal.First();
            forumtotals.PostCount += 1;
            await _dbContext.SaveChangesAsync();
            return post.Id;
        }
        /// <summary>
        /// Create a Reply
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public int CreateForMerge(int[]? selected)
        {
            var topics = _dbContext.Posts.AsNoTracking().Where(p => selected.Contains(p.Id)).OrderBy(t => t.Created).ToList();
            int unmoderatedposts = 0;
            int replycounter = 0;
            Post mainTopic = topics.First();
            int maintopicid = mainTopic.Id;
            unmoderatedposts += mainTopic.UnmoderatedReplies;
            Forum forum = _dbContext.Forums.Find(mainTopic.ForumId);
            Forum oldforum = null;
            foreach (Post topic in topics)
            {
                if (topic.Id != mainTopic.Id)
                {
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
                    //Update any subscriptionc for the old Topic with the new topic ID
                    _dbContext.Database.ExecuteSql($"UPDATE {_tableprefix}SUBSCRIPTIONS SET TOPIC_ID={mainTopic.Id}, FORUM_ID={mainTopic.ForumId}, CAT_ID={mainTopic.CategoryId} WHERE TOPIC_ID={topic.Id}");
                    //Update the replies with new topic ID and add the number updated to the replycounter
                    replycounter += _dbContext.Database.ExecuteSql(
                        $"UPDATE {_tableprefix}REPLY SET TOPIC_ID={mainTopic.Id},FORUM_ID={mainTopic.ForumId},CAT_ID={mainTopic.CategoryId} WHERE TOPIC_ID={topic.Id}");
                    if (oldforum != null)
                    {
                        _forumservice.UpdateLastPost(oldforum.Id);
                    }

                    if (_config.GetIntValue("STRMOVENOTIFY") == 1)
                        _mailSender.TopicMergeEmail(topic, mainTopic,topic.Member);
                }
            }

            mainTopic.ReplyCount += replycounter;
            _dbContext.Update(mainTopic);
            _dbContext.SaveChanges();
            //update counts
            UpdateLastPost(mainTopic.Id,unmoderatedposts);
            _forumservice.UpdateLastPost(forum.Id);
            return maintopicid;
        }
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

        public async Task DeleteTopic(int id)
        {
            var post = _dbContext.Posts.Include(p=>p.Replies).SingleOrDefault(f => f.Id == id);
            if (post != null)
            {
                var forumid = post.ForumId;
                _dbContext.Posts.Remove(post);
                await _dbContext.SaveChangesAsync();

                await _forumservice.UpdateLastPost(forumid);
            }
        }
        public async Task DeleteReply(int id)
        {
            var post = _dbContext.Replies.SingleOrDefault(f => f.Id == id);
            int? moderated = null;
            if (post.Status == (short)Status.UnModerated || post.Status == (short)Status.OnHold)
            {
                moderated = -1;
            }
            if (post != null)
            {
                var topicid = post.PostId;
                var forumid = post.ForumId;
                _dbContext.Replies.Remove(post);
                await _dbContext.SaveChangesAsync();

                await UpdateLastPost(topicid, moderated);
                await _forumservice.UpdateLastPost(forumid);

            }
        }

        public async Task Update(Post post)
        {
            if (_config.GetIntValue("STRBADWORDFILTER") == 1)
            {
                var badwords = _dbContext.Badwords.AsNoTracking()
                    .ToDictionary(t=>t.Word);
                foreach (var badword in badwords)
                {
                    post.Content.Replace(badword.Key, badword.Value.ReplaceWith);
                }
            }
            _dbContext.Update(post);
            await _dbContext.SaveChangesAsync();
        }
        public async Task Update(PostReply post)
        {
            if (_config.GetIntValue("STRBADWORDFILTER") == 1)
            {
                var badwords = _dbContext.Badwords.AsNoTracking()
                    .ToDictionary(t=>t.Word);
                foreach (var badword in badwords)
                {
                    post.Content.Replace(badword.Key, badword.Value.ReplaceWith);
                }
            }
            _dbContext.Update(post);
            await _dbContext.SaveChangesAsync();
        }
        public async Task UpdateViewCount(int id)
        {
            var topic = _dbContext.Posts.First(x=>x.Id == id);
            topic.ViewCount += 1;
            _dbContext.Update(topic);
            await _dbContext.SaveChangesAsync();
        }
        public async Task UpdateTopicContent(int id, string content)
        {
            var topic = _dbContext.Posts.First(x=>x.Id == id);
            topic.Content = content;
            _dbContext.Update(topic);
            await _dbContext.SaveChangesAsync();
        }
        public async Task UpdateReplyContent(int id, string content)
        {
            var reply = _dbContext.Replies.First(x=>x.Id == id);
            reply.Content = content;
            _dbContext.Update(reply);
            await _dbContext.SaveChangesAsync();
        }

        public IEnumerable<Post> GetLatestPosts(int n)
        {
            return _dbContext.Posts
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Forum)
                .Include(p => p.Member).OrderByDescending(post => post.LastPostDate).Take(n);
        }
        public IPagedList<PostReply> GetPagedReplies(int topicid, int pagesize = 10, int pagenumber = 1)
        {
            var replies = _dbContext.Replies.Where(p => p.PostId == topicid)
                .AsNoTrackingWithIdentityResolution()
                .Include(p => p.Member).AsNoTracking()
                .Skip((pagenumber-1) * pagesize).Take(pagesize);
            return replies.ToPagedList(pagenumber, pagesize);
        }
        public IEnumerable<Post> GetAllTopicsAndRelated()
        {
            return _dbContext.Posts
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Forum)
                .Include(p => p.Member)
                .Include(p=>p.LastPostAuthor);
        }

        public Post? GetTopic(int id)
        {
            var post = _dbContext.Posts
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Forum)
                .Include(p => p.Member)
                .SingleOrDefault(p => p.Id == id);
            return post;
        }
        public Post GetTopicForUpdate(int id)
        {
            var post = _dbContext.Posts
                .AsNoTrackingWithIdentityResolution()
                .Single(p => p.Id == id);
            return post; 
        }
        public ArchivedPost GetArchivedTopic(int id)
        {
            var post = _dbContext.ArchivedTopics
                .AsNoTracking()
                .Include(p => p.Category).AsNoTracking()
                .Include(p => p.Forum).AsNoTracking()
                .Include(p => p.Member).AsNoTracking()
                .Single(p => p.Id == id);
            return post;
        }
        public Post? GetTopicWithRelated(int id)
        {

            var post = _dbContext.Posts.Where(p => p.Id == id)
                .AsNoTrackingWithIdentityResolution()
                .Include(p => p.Member).AsNoTracking()
                .Include(p => p.LastPostAuthor).AsNoTracking()
                .Include(p => p.Category).AsNoTracking()
                .Include(p => p.Forum).AsNoTracking()
                .Include(p => p.Replies!.OrderByDescending(r => r.Created))
                .ThenInclude(r => r.Member).AsNoTracking()
                //.AsSplitQuery()
                .SingleOrDefault();

            return post;
        }
        public ArchivedPost GetArchivedTopicWithRelated(int id)
        {

            var post = _dbContext.ArchivedTopics.Where(p => p.Id == id)
                .AsNoTracking()
                .Include(p => p.Member).AsNoTracking()
                .Include(p => p.LastPostAuthor).AsNoTracking()
                .Include(p => p.Category).AsNoTracking()
                .Include(p => p.Forum).AsNoTracking()
                .Include(p => p.Replies!.OrderByDescending(r => r.Created))
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
                .Include(r => r.Topic).ThenInclude(t=>t.Member).AsNoTracking()
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
                return new PagedList<Post>(null,page,pagesize);
            }
            var posts = _dbContext.Posts.Where(p=>p.Title.Contains(searchQuery) || p.Content.Contains(searchQuery));

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
                .Include(p=>p.Replies).ThenInclude(r=>r.Member)
                .AsQueryable();

            if (searchQuery.SinceDate != SearchDate.AnyDate)
            {
                var lastvisit = DateTime.UtcNow.AddDays(-(int)searchQuery.SinceDate);
                posts = posts
                    
                .Where(f => f.LastPostDate.FromForumDateStr() > lastvisit)
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

        public Post GetLatestReply(int id)
        {
            throw new NotImplementedException();
        }
        public async Task UpdateLastPost(int topicid, int? moderatedcount)
        {
            var count = _dbContext.Replies.Count(r => r.PostId == topicid && r.Status < 2);
            var topic = GetTopicForUpdate(topicid);
            var lastreply = _dbContext.Replies.AsNoTrackingWithIdentityResolution()
                .Where(t=>t.PostId == topicid && t.Status < 2)
                .OrderByDescending(t=>t.Created)
                .Select(p => new { LastPostId = p.Id, LastPostAuthorId = p.MemberId,LastPostDate = p.Created, PostCount = _dbContext.Replies.Count(r=>r.PostId == topicid && r.Status <2) })
                .FirstOrDefault();

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
                topic.ReplyCount = lastreply.PostCount;
            }

            if (moderatedcount != null)
            {
                topic.UnmoderatedReplies += moderatedcount.Value;
            }
            _dbContext.Update(topic);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> Answer(int id)
        {
            var reply = _dbContext.Replies.Where(r => r.Id == id).FirstOrDefault();
            if (reply != null)
            {
                var topic = _dbContext.Posts.Where(t => t.Id == reply.PostId).FirstOrDefault();
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

        public bool HasPoll(int id)
        {
            return _dbContext.Polls.SingleOrDefault(p=>p.TopicId == id) != null;
        }

        public Poll? GetPoll(int topicid)
        {
            return _dbContext.Polls.Include(p=>p.PollAnswers).SingleOrDefault(p=>p.TopicId ==topicid);
        }

        public List<Post> GetById(int[] ids)
        {
            return _dbContext.Posts.AsNoTracking().Where(p => ids.Contains(p.Id)).OrderBy(t => t.Created).ToList();
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
            var forum = (from forums in _dbContext.Forums
                    select forums).SingleOrDefault(f => f.Id == forumId);
            int replycount = 0;
            int originaltopicid = 0;
            Post? topic = null;
            bool first = true;
            try
            {
                _dbContext.Database.BeginTransaction();
                foreach (string id in ids.OrderByDescending(s => s))
                {
                    //fetch the reply
                    var reply = (from replies in _dbContext.Replies
                        select replies).SingleOrDefault(r=>r.Id == Convert.ToInt32(id));

                    if (first)
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
                            LastPostReplyId = 0,
                            Status =(short)Status.Open,
                            LastPostDate = reply.Created,
                            ReplyCount = 0
                        };

                        _dbContext.Add(topic);
                        _dbContext.Remove(reply);
                        _dbContext.SaveChanges();
                        first = false;
                    }
                    else
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

                topic.ReplyCount = replycount;
                _dbContext.Update(topic);
                _dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                _dbContext.Database.RollbackTransaction();
                throw;
            }        
            finally
            {
                _dbContext.Database.CommitTransaction();
            }
            Post originaltopic = (from t in _dbContext.Posts select t).FirstOrDefault(t=>t.Id == originaltopicid);
            if (originaltopic != null)
            {
                originaltopic.ReplyCount -= replycount + 1;
                _dbContext.Update(originaltopic);
                _dbContext.SaveChanges();
                UpdateLastPost(originaltopicid,0);
                _forumservice.UpdateLastPost(originaltopic.ForumId);
            }
            return topic;
        }
    }
}
