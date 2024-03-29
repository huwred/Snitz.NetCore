using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace SnitzCore.Service
{
    public class ForumService : IForum
    {
        private readonly SnitzDbContext _dbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        
        public ForumService(SnitzDbContext dbContext,RoleManager<IdentityRole> roleManager)
        {
            _dbContext = dbContext;
            _roleManager = roleManager;
        }

        public async Task Create(Forum forum)
        {
            _dbContext.Forums.Add(forum);
            await _dbContext.SaveChangesAsync();

            if (forum.Privateforums is not ForumAuthType.All)
            {
                await _roleManager.CreateAsync(new IdentityRole($"Forum_{forum.Id}"));
            }
            
        }
        public async Task Delete(int forumId)
        {
            try
            {
                await _dbContext.Posts.Where(p=>p.ForumId == forumId).Include(t=>t.Replies).ExecuteDeleteAsync();
                await _dbContext.Forums.Where(f => f.Id == forumId).ExecuteDeleteAsync();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
        public async Task Update(Forum forum)
        {

            var updForum = await _dbContext.Forums.FindAsync(forum.Id);
            if (updForum != null)
            {
                updForum.Title = forum.Title;
                updForum.Description = forum.Description;
                updForum.CategoryId = forum.CategoryId;
                updForum.Type = forum.Type;
                updForum.Privateforums = forum.Privateforums;
                updForum.Status = forum.Status;
                updForum.Order = forum.Order;
                updForum.Defaultdays = forum.Defaultdays;
                updForum.CountMemberPosts = forum.CountMemberPosts;
                updForum.Moderation = forum.Moderation;
                _dbContext.Update(updForum);
            }

            await _dbContext.SaveChangesAsync();

            if (_roleManager.RoleExistsAsync($"Forum_{forum.Id}").Result)
            {
                if (forum.Privateforums is ForumAuthType.All)
                {
                    await _roleManager.DeleteAsync(new IdentityRole($"Forum_{forum.Id}"));
                }
            }
            else if (forum.Privateforums is not ForumAuthType.All)
            {
                await _roleManager.CreateAsync(new IdentityRole($"Forum_{forum.Id}"));
            }
        }
        public async Task UpdatePostCount(int forumId, int topic = 0, int reply = 0)
        {
            var forum = _dbContext.Forums.SingleOrDefault(f => f.Id == forumId);
            if (forum != null)
            {
                forum.TopicCount += topic;
                forum.ReplyCount += reply;
                _dbContext.Forums.Update(forum);
            }

            await _dbContext.SaveChangesAsync();
        }
        public async Task UpdateForumTitle(int forumId, string newTitle)
        {
            var forum = _dbContext.Forums.SingleOrDefault(f => f.Id == forumId);
            if (forum != null)
            {
                forum.Title = newTitle;
                _dbContext.Forums.Update(forum);
            }

            await _dbContext.SaveChangesAsync();
        }
        public async Task UpdateForumDescription(int forumId, string newDescription)
        {
            var forum = _dbContext.Forums.SingleOrDefault(f => f.Id == forumId);
            if (forum != null)
            {
                forum.Description = newDescription;
                _dbContext.Forums.Update(forum);
            }

            await _dbContext.SaveChangesAsync();
        }
        public IPagedList<Post> GetPagedTopics(int id, int pagesize = 10, int page = 1)
        {

            var posts = _dbContext.Posts.Where(f => f.ForumId == id).AsNoTracking()
                .Include(p => p.Member)
                .OrderByDescending(p => p.Created);

            return posts.ToPagedList(page, pagesize);

        }
        public IEnumerable<Forum> GetAll()
        {
            var forums = _dbContext.Forums
                .Include(forum=>forum.Category)
                .AsNoTracking()
                .OrderBy(forum=>forum.Category!.Sort).ThenBy(forum=>forum.Order);

            return forums;
        }
        public Dictionary<int, string?> CategoryList()
        {
            return _dbContext.Categories.AsNoTracking().Select(c=> new {c.Id, value = c.Name}).ToDictionary(k=>k.Id,k=>k.value);
        }
        public Post? GetLatestPost(int forumId)
        {
            return _dbContext.Posts.Where(f => f.Id == forumId)
                .AsNoTracking()
                .Include(p => p.Member)
                
                .OrderByDescending(p=>p.Created)
                .FirstOrDefault();
        }
        public Forum GetById(int id)
        {
            return _dbContext.Forums.Where(f => f.Id == id)
                .Include(f=>f.Category)
                
                .Include(f => f.Posts!.OrderByDescending(p => p.Created))
                .ThenInclude(p => p.Member)
                .Include(f=>f.ForumModerators)!
                .ThenInclude(p => p.Member)
                .AsNoTracking()
                .Single();

        }
        public Dictionary<int, string> ForumList()
        {
            return _dbContext.Forums.AsNoTracking().Where(f=>f.Privateforums == ForumAuthType.All).OrderBy(f=>f.Title).Select(c=> new {c.Id, value = c.Title}).ToDictionary(k=>k.Id,k=>k.value);
        }

        public string ForumName(string rolename)
        {
            var id = rolename.ToUpperInvariant().Replace("FORUM_","");
            var result = 
                _dbContext.Forums.FirstOrDefault(f => f.Id == Convert.ToInt32(id));

            if (result != null) return result.Title;
            return String.Empty;
        }

        public async Task EmptyForum(int id)
        {
            await _dbContext.Posts.Where(p => p.ForumId == id).Include(t => t.Replies).ExecuteDeleteAsync();
            var forum = await _dbContext.Forums.FindAsync(id);
            if (forum != null)
            {
                forum.LastPost = null;
                forum.LastPostAuthorId = null;
                forum.LatestReplyId = null;
                forum.LatestTopicId = null;
                forum.ReplyCount = 0;
                forum.TopicCount = 0;
                _dbContext.Forums.Update(forum);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<Forum> UpdateLastPost(int forumid)
        {
            var forum = await _dbContext.Forums.FirstAsync(f => f.Id == forumid);

            var lasttopic = _dbContext.Posts
                .Where(t=>t.ForumId == forumid && t.Status < 2)
                .OrderByDescending(t=>t.LastPostDate)
                .Select(p => new { Post = p, Topics = _dbContext.Posts.Count(t=>t.ForumId == forumid && t.Status <2), Replies = _dbContext.Replies.Count(r=>r.ForumId == forumid && r.Status <2) })
                .FirstOrDefault();

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
                forum.LatestTopicId = lasttopic.Post.Id;
                forum.LatestReplyId = lasttopic.Post.LastPostReplyId;
                forum.LastPost = lasttopic.Post.LastPostDate;
                forum.LastPostAuthorId = lasttopic.Post.LastPostAuthorId;
                forum.TopicCount = lasttopic.Topics;
                forum.ReplyCount = lasttopic.Replies;
            }

            _dbContext.Forums.Update(forum);
            await _dbContext.SaveChangesAsync();
            return forum;
            //var cacheService = new InMemoryCache();
            //cacheService.Remove("category.forums");           
        }

        public PagedList<Post> FetchMyForumTopics(int pagesize, int pagenum, IEnumerable<int> forumids)
        {
            var result = _dbContext.Posts.AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Forum)
                .Include(p => p.Member).Include(p => p.LastPostAuthor)
                .Where(p=> forumids.Contains(p.ForumId))
                .Where(p=>p.Status < 2)
                .OrderByDescending(p=>p.LastPostDate);
            return new PagedList<Post>(result, pagenum, pagesize);
        }

        public IEnumerable<string> GetTagStrings(List<int> list)
        {
            return _dbContext.Posts.Where(f => list.Contains(f.ForumId)).Select(p => p.Content);

        }
        [OutputCache(Duration = 3600)]
        public IEnumerable<MyViewTopic> FetchAllMyForumTopics(IEnumerable<int> forumids)
        {
            var result = _dbContext.Posts.AsNoTracking()
                .Include(p => p.Forum)
                .Where(p=> forumids.Contains(p.ForumId))
                .Where(p=>p.Status < 2)
                .OrderByDescending(p=>p.LastPostDate)
                .Select(post => new MyViewTopic
                {
                    Id = post.Id,
                    Subject = post.Title,
                    Date = post.Created,
                    LastPost = post.LastPostDate
                });
            return result;
        }

        public int PollsAuth(int id)
        {
            try
            {
                var auth = _dbContext.Database.SqlQuery<int>(
                    $"SELECT F_ALLOWEVENTS AS Value FROM FORUM_FORUM WHERE FORUM_ID={id}").Single();
                return auth;
            }
            catch (Exception e)
            {

                return 0;
            }

        }
    }
}
