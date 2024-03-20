using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto.Operators;
using X.PagedList;
using SkiaSharp;

namespace SnitzCore.Service
{
    public class PostService : IPost
    {
        private readonly SnitzDbContext _dbContext;
        private readonly IMember _memberService;
        private readonly IForum _forumservice;

        public PostService(SnitzDbContext dbContext, IMember memberService,IForum forumservice)
        {
            _dbContext = dbContext;
            _memberService = memberService;
            _forumservice = forumservice;

        }

        /// <summary>
        /// Create a Topic
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public async Task Create(Post post)
        {
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

        }

        /// <summary>
        /// Create a Reply
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public async Task Create(PostReply post)
        {
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
            }
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
            _dbContext.Update(post);
            await _dbContext.SaveChangesAsync();
        }
        public async Task Update(PostReply post)
        {
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
            return GetAllTopicsAndRelated().OrderByDescending(post => post.LastPostDate).Take(n);
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
                .AsNoTrackingWithIdentityResolution()
                .Include(p => p.Category).AsNoTracking()
                .Include(p => p.Forum).AsNoTracking()
                .Include(p => p.Member).AsNoTracking();
        }

        public Post GetTopic(int id)
        {
            var post = _dbContext.Posts
                .AsNoTrackingWithIdentityResolution()
                .Include(p => p.Category).AsNoTracking()
                .Include(p => p.Forum).AsNoTracking()
                .Include(p => p.Member).AsNoTracking()
                .Single(p => p.Id == id);
            return post;
        }
        public Post GetTopicForUpdate(int id)
        {
            var post = _dbContext.Posts
                .AsNoTrackingWithIdentityResolution()
                .Single(p => p.Id == id);
            return post; 
        }
        public ArchivedTopic GetArchivedTopic(int id)
        {
            var post = _dbContext.ArchivedTopics
                .AsNoTracking()
                .Include(p => p.Category).AsNoTracking()
                .Include(p => p.Forum).AsNoTracking()
                .Include(p => p.Member).AsNoTracking()
                .Single(p => p.Id == id);
            return post;
        }
        public Post GetTopicWithRelated(int id)
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
                .Single();

            return post;
        }
        public ArchivedTopic GetArchivedTopicWithRelated(int id)
        {

            var post = _dbContext.ArchivedTopics.Where(p => p.Id == id)
                .AsNoTrackingWithIdentityResolution()
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
                .AsNoTrackingWithIdentityResolution()
                .Single();

            return post;
        }
        public IPagedList<Post> GetFilteredPost(string? searchQuery,out int totalcount, int pagesize=25, int page=1)
        {
            if (searchQuery == null)
            {
                totalcount = 0;
                return new PagedList<Post>(null,page,pagesize);
            }
            var posts = _dbContext.Posts.Where(p=>p.Title.Contains(searchQuery) || p.Content.Contains(searchQuery));

            posts = posts.Include(p => p.Forum).OrderByDescending(p=>p.LastPostDate??p.Created);
            totalcount = posts.Count();
            return posts.ToPagedList(page, pagesize);
        }

        public IPagedList<Post> Find(ForumSearch searchQuery, out int totalcount, int pagesize, int page)
        {

            var posts = _dbContext.Posts.Include(p=>p.Forum).AsQueryable();

            if (searchQuery.SearchCategory is > 0)
            {
                posts = posts.Where(p => p.CategoryId == searchQuery.SearchCategory);
            }
            if (searchQuery.SearchForums != null && searchQuery.SearchForums.Any())
            {
                posts = posts.Where(p => searchQuery.SearchForums.Contains(p.ForumId));
            }
            if (!string.IsNullOrWhiteSpace(searchQuery.UserName))
            {
                posts = posts.Where(p => p.Member!.Name.ToLower().StartsWith(searchQuery.UserName.ToLower()))
                    .Include(p => p.Member)
                    .Include(p => p.Forum);

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

            if (searchQuery.SinceDate != SearchDate.AnyDate)
            {
                posts = posts.Where(p =>
                    p.Created.FromForumDateStr() > DateTime.UtcNow.AddDays(-(int)searchQuery.SinceDate));
            }
            totalcount = posts.Count();
            return posts.OrderBy(p=>p.LastPostDate).ToPagedList(page, pagesize);
        }

        public Post GetLatestReply(int id)
        {
            throw new NotImplementedException();
        }
        public async Task UpdateLastPost(int topicid, int? moderatedcount)
        {
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
    }
}
