using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using System.Collections.Generic;
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

            var myObj = await _dbContext.Forums.FindAsync(forum.Id);
            if (myObj != null)
            {
                myObj.Title = forum.Title;
                myObj.Description = forum.Description;
                myObj.CategoryId = forum.CategoryId;
                myObj.Type = forum.Type;
                myObj.Privateforums = forum.Privateforums;
                myObj.Status = forum.Status;
                myObj.Order = forum.Order;
                myObj.Defaultdays = forum.Defaultdays;
                myObj.CountMemberPosts = forum.CountMemberPosts;
                _dbContext.Update(myObj);
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
            return _dbContext.Forums.AsNoTracking().Where(f=>f.Privateforums == ForumAuthType.All).Select(c=> new {c.Id, value = c.Title}).ToDictionary(k=>k.Id,k=>k.value);
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
    }
}
