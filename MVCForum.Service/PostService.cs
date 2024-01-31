using Microsoft.EntityFrameworkCore;
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

        public PostService(SnitzDbContext dbContext, IMember memberService)
        {
            _dbContext = dbContext;
            _memberService = memberService;

        }

        /// <summary>
        /// Create a Topic
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public async Task Create(Post post)
        {
            post.LastPostDate = post.Created;
            post.Status = 0;
            post.LastPostAuthorId = post.MemberId;
            await _dbContext.Posts.AddAsync(post);
            await _dbContext.SaveChangesAsync();

            UpdateForumLastPost(post);
            await _dbContext.SaveChangesAsync();

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
            //update topic stuff
            var topic = _dbContext.Posts.Single(p => p.Id == post.PostId);
            topic.LastPostAuthorId = post.MemberId;
            topic.ReplyCount += 1;
            topic.LastPostReplyId = post.Id;
            topic.LastPostDate = post.Created;
            _dbContext.Posts.Update(topic);
            await _dbContext.SaveChangesAsync();
            //update Forum
            await UpdateForumLastPost(post);
            
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
                var replycount = post.ReplyCount;

                _dbContext.Posts.Remove(post);
                await _dbContext.SaveChangesAsync();
                
                var forum = _dbContext.Forums.SingleOrDefault(f => f.Id == forumid);
                if (forum != null)
                {
                    if (forum.LatestTopicId == id)
                    {
                        var lasttopic = _dbContext.Posts.Where(f=>f.ForumId == forumid).OrderByDescending(r=>r.Created).FirstOrDefault();
                        if (lasttopic != null)
                        {
                            forum.LatestTopicId = lasttopic.Id;
                            forum.TopicCount -= 1;
                            forum.ReplyCount -= replycount;
                            forum.LatestReplyId = lasttopic.LastPostReplyId;
                            forum.LastPostAuthorId = lasttopic.LastPostAuthorId;
                            forum.LastPost = lasttopic.LastPostDate;
                            _dbContext.Update(forum);
                        }
                        else
                        {
                            forum.LatestTopicId = null;
                            forum.LatestReplyId = null;
                            forum.LastPostAuthorId = null;
                            forum.LastPost = null;
                            forum.ReplyCount = 0;
                            forum.TopicCount = 0;
                            _dbContext.Update(forum);
                        }
                    }
                }
            }
            await _dbContext.SaveChangesAsync();
        }
        public async Task DeleteReply(int id)
        {
            var post = _dbContext.Replies.SingleOrDefault(f => f.Id == id);
            if (post != null)
            {
                var topicid = post.PostId;
                var forumid = post.ForumId;

                _dbContext.Replies.Remove(post);
                await _dbContext.SaveChangesAsync();
                var topic = _dbContext.Posts.SingleOrDefault(f => f.Id == topicid);
                if (topic != null)
                {
                    topic.ReplyCount -= 1;
                    if (topic.LastPostReplyId == id)
                    {
                        var lastreply = _dbContext.Replies.Where(f=>f.PostId == topicid).OrderByDescending(r=>r.Created).FirstOrDefault();
                        if (lastreply != null)
                        {
                            topic.LastPostReplyId = lastreply.Id;
                            topic.LastPostAuthorId = lastreply.MemberId;
                            topic.LastPostDate = lastreply.Created;
                        }
                        else
                        {
                            topic.ReplyCount = 0;
                            topic.LastPostReplyId = null;
                            topic.LastPostAuthorId = null;
                            topic.LastPostDate = null;
                        }

                        _dbContext.Update(topic);
                        await _dbContext.SaveChangesAsync();
                    }
                    var forum = _dbContext.Forums.SingleOrDefault(f => f.Id == forumid);

                    if (forum != null)
                    {
                        if (forum.LatestReplyId == id)
                        {
                            forum.LatestReplyId = topic.LastPostReplyId;
                            forum.ReplyCount -= 1;
                            forum.LastPostAuthorId = topic.LastPostAuthorId ?? topic.MemberId;
                            _dbContext.Update(forum);
                            await _dbContext.SaveChangesAsync();
                        }
                    }                    
                }

            }
            await _dbContext.SaveChangesAsync();
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
            return GetAllTopicsAndRelated().OrderByDescending(post => post.Created).Take(n);
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
        public PostReply GetReply(int id)
        {

            var post = _dbContext.Replies.Where(p => p.Id == id)
                .AsNoTrackingWithIdentityResolution()
                .Include(p => p.Member).AsNoTracking()
                .Include(r => r.Topic).AsNoTracking()
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

            posts = posts.Include(p => p.Forum);
            totalcount = posts.Count();
            return posts.ToPagedList(page, pagesize);
        }

        public IPagedList<Post> Find(ForumSearch searchQuery, out int totalcount, int pagesize, int page)
        {

            var posts = _dbContext.Posts.AsQueryable();

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
            return posts.ToPagedList(page, pagesize);
        }

        public Post GetLatestReply(int id)
        {
            throw new NotImplementedException();
        }
        private async Task UpdateForumLastPost(Post post)
        {
            Forum forum = _dbContext.Forums.Single(f => f.Id == post.ForumId);
            await _dbContext.SaveChangesAsync();
            forum.LastPost = post.Created;
            forum.LatestTopicId = post.Id;
            forum.LastPostAuthorId = post.MemberId;
            forum.TopicCount += 1;
            _dbContext.Forums.Update(forum);
            if (forum.CountMemberPosts == 1)
            {
                UpdateMemberPosts(post.MemberId);
            }
        }

        private async Task UpdateMemberPosts(int postauthor)
        {
            var member = _memberService.GetById(postauthor);
            await _dbContext.SaveChangesAsync();
            if (member != null)
            {
                member.Posts += 1;
                member.Lastpostdate = DateTime.UtcNow.ToForumDateStr();
                member.Lastactivity = DateTime.UtcNow.ToForumDateStr();
                _dbContext.Members.Update(member);
                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task UpdateForumLastPost(PostReply post)
        {

            var forum = _dbContext.Forums.AsNoTracking().Single(f => f.Id == post.ForumId);
            await _dbContext.SaveChangesAsync();
            forum.LastPost = post.Created;
            forum.LatestTopicId = post.PostId;
            forum.LatestReplyId = post.Id;
            forum.LastPostAuthorId = post.MemberId;
            forum.ReplyCount += 1;
            _dbContext.Forums.Update(forum);
            await _dbContext.SaveChangesAsync();
            if (forum.CountMemberPosts == 1)
            {
                await UpdateMemberPosts(post.MemberId);
            }
        }
    }
}
