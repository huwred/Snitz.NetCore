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
            post.Status = 1;
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
            var topic = GetTopic(post.PostId);
            topic.LastPostAuthorId = post.MemberId;
            topic.ReplyCount += 1;
            topic.LastPostReplyId = post.Id;
            topic.LastPostDate = post.Created;
            _dbContext.Posts.Update(topic);
            await _dbContext.SaveChangesAsync();
            //update Forum
            UpdateForumLastPost(post);
            await _dbContext.SaveChangesAsync();
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
            if (post != null) _dbContext.Posts.Remove(post);
            await _dbContext.SaveChangesAsync();
        }
        public async Task DeleteReply(int id)
        {
            var post = _dbContext.Replies.SingleOrDefault(f => f.Id == id);
            if (post != null) _dbContext.Replies.Remove(post);
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
                .Include(p => p.Member)
                .Skip((pagenumber-1) * pagesize).Take(pagesize);
            return replies.ToPagedList(pagenumber, pagesize);
        }
        public IEnumerable<Post> GetAllTopicsAndRelated()
        {
            return _dbContext.Posts
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Forum)
                .Include(p => p.Member);
            ;
            //.Include(p => p.Replies)!
            //.ThenInclude(r => r.Member);

        }

        public Post GetTopic(int id)
        {
            var post = _dbContext.Posts.Single(p => p.Id == id);
            return post;
        }
        public Post GetTopicWithRelated(int id)
        {

            var post = _dbContext.Posts.Where(p => p.Id == id)
                .AsNoTracking()
                .Include(p => p.Member)
                .Include(p => p.LastPostAuthor)
                .Include(p => p.Category)
                .Include(p => p.Forum)
                .Include(p => p.Replies!.OrderByDescending(r => r.Created))
                .ThenInclude(r => r.Member)
                .AsSplitQuery()
                .Single();

            return post;
        }
        public PostReply GetReply(int id)
        {

            var post = _dbContext.Replies.Where(p => p.Id == id)
                .AsNoTracking()
                .Include(p => p.Member)
                .Include(r => r.Topic)
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

            if (searchQuery.SearchCategory != null && searchQuery.SearchCategory > 0)
            {
                posts = posts.Where(p => p.CategoryId == searchQuery.SearchCategory);
            }
            if (searchQuery.SearchForums.Any())
            {
                posts = posts.Where(p => searchQuery.SearchForums.Contains(p.ForumId));
            }
            if (!string.IsNullOrWhiteSpace(searchQuery.UserName))
            {
                posts = posts.Where(p => p.Member!.Name.ToLower().StartsWith(searchQuery.UserName.ToLower()))
                    .Include(p => p.Member)
                    .Include(p => p.Forum);

            }
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
        private void UpdateForumLastPost(Post post)
        {
            Forum forum = _dbContext.Forums.Single(f => f.Id == post.ForumId);
            forum.LastPost = post.Created;
            forum.LatestTopicId = post.Id;
            forum.LastPostAuthorId = post.Member?.Id;
            forum.TopicCount += 1;
            _dbContext.Forums.Update(forum);
            if (forum.CountMemberPosts == 1)
            {
                UpdateMemberPosts(post.MemberId);
            }
        }

        private void UpdateMemberPosts(int postauthor)
        {
            var member = _memberService.GetById(postauthor).Result;
            member.Posts += 1;
            member.Lastpostdate = DateTime.UtcNow.ToForumDateStr();
            member.Lastactivity = DateTime.UtcNow.ToForumDateStr();
            _dbContext.Members.Update(member);
        }

        private void UpdateForumLastPost(PostReply post)
        {
            var forum = _dbContext.Forums.Single(f => f.Id == post.ForumId);
            forum.LastPost = post.Created;
            forum.LatestTopicId = post.PostId;
            forum.LatestReplyId = post.Id;
            forum.LastPostAuthorId = post.MemberId;
            forum.ReplyCount += 1;
            _dbContext.Forums.Update(forum);
            if (forum.CountMemberPosts == 1)
            {
                UpdateMemberPosts(post.MemberId);
            }
        }
    }
}
