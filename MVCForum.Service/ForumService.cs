using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
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
using System.Threading.Tasks;
using X.PagedList;
using X.PagedList.Extensions;
using static Azure.Core.HttpHeader;

namespace SnitzCore.Service
{
    public class ForumService : IForum
    {
        private readonly SnitzDbContext _dbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly string? _tableprefix;
        private readonly log4net.ILog _logger;

        public ForumService(SnitzDbContext dbContext,RoleManager<IdentityRole> roleManager,IOptions<SnitzForums> config)
        {
            _dbContext = dbContext;
            _roleManager = roleManager;
            _tableprefix = config.Value.forumTablePrefix;
            _logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType!);

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
                updForum.Subscription = forum.Subscription;
                updForum.Password = forum.Password;
                updForum.Rating = forum.Rating;
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
            //var forums = _dbContext.Forums
            //    .AsNoTracking()
            //    .Include(f=>f.Category)
            //    .OrderBy(forum=>forum.Category!.Sort).ThenBy(forum=>forum.Order);
            return CacheProvider.GetOrCreate("AllForums", () => _dbContext.Forums
                .AsNoTracking()
                .Include(f=>f.Category)
                .OrderBy(forum=>forum.Category!.Sort).ThenBy(forum=>forum.Order).ToList(), TimeSpan.FromMinutes(10));
            //return forums;
        }
        public Dictionary<int, string?> CategoryList()
        {
            return CacheProvider.GetOrCreate("CatList", () => _dbContext.Categories.AsNoTracking().OrderBy(c=>c.Sort).Select(c=> new {c.Id, value = c.Name}).ToDictionary(k=>k.Id,k=>k.value), TimeSpan.FromMinutes(60));
            //return _dbContext.Categories.AsNoTracking().OrderBy(c=>c.Sort).Select(c=> new {c.Id, value = c.Name}).ToDictionary(k=>k.Id,k=>k.value);
        }
        public Post? GetLatestPost(int forumId)
        {
            return _dbContext.Posts.Where(f => f.Id == forumId)
                .AsNoTracking()
                .Include(p => p.Member)
                
                .OrderByDescending(p=>p.Created)
                .FirstOrDefault();
        }
        public Forum Get(int id)
        {
            try
            {
                return  _dbContext.Forums.AsNoTracking().Include(f=>f.Category).Single(f=>f.Id==id);
            }
            catch (Exception)
            {

                throw new KeyNotFoundException($"Forum {id} not found");
            }
        }
        public Forum GetWithPosts(int id)
        {
            var f = _dbContext.Forums.AsNoTracking().Include(f=>f.Category).Include(f => f.Posts).Single(f => f.Id == id);
            if (f.Posts != null)
            {
                return _dbContext.Forums.AsNoTracking().Where(f => f.Id == id)
                    .Include(f=>f.Category)              
                    .Include(f => f.Posts!)
                    .ThenInclude(p => p.Member)
                    .Include(f=>f.ForumModerators)!
                    .ThenInclude(p => p.Member)
                    .AsSplitQuery()
                    .Single();
            }
            return f;

        }
        public Dictionary<int, string> ForumList(bool admin = true)
        {
            if (admin)
            {
                return CacheProvider.GetOrCreate("ForumList", () => _dbContext.Forums.AsNoTracking().OrderBy(f=>f.Title).Select(c=> new {c.Id, value = c.Title}).ToDictionary(k=>k.Id,k=>k.value),TimeSpan.FromDays(1));
            }
            return CacheProvider.GetOrCreate("ForumList", () => _dbContext.Forums.AsNoTracking().Where(f=>f.Privateforums == ForumAuthType.All).OrderBy(f=>f.Title).Select(c=> new {c.Id, value = c.Title}).ToDictionary(k=>k.Id,k=>k.value),TimeSpan.FromDays(1));
        }
        public Dictionary<int, string> ModeratedForums()
        {
            return _dbContext.Forums.AsNoTracking().Where(f=>f.Moderation != Moderation.UnModerated).OrderBy(f=>f.Title).Select(c=> new {c.Id, value = c.Title}).ToDictionary(k=>k.Id,k=>k.value);
        }
        public string ForumName(string rolename)
        {
            var id = rolename.ToUpperInvariant().Replace("FORUM_","");
            var result = 
                _dbContext.Forums.AsNoTracking().OrderBy(f=>f.Id).FirstOrDefault(f => f.Id == Convert.ToInt32(id));

            if (result != null) return result.Title;
            return "Forum no longer exists!";
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
            Post? lasttopic = null;
            int topiccount = 0;
            int replycount = 0;
            try
            {
                var topics = _dbContext.Posts.AsNoTracking().Where(t=>t.ForumId == forumid && t.Status < 2).AsEnumerable();
                if (topics.Count() > 0)
                {
                    lasttopic = topics.OrderByDescending(t => t.LastPostDate).FirstOrDefault();
                }
                topiccount = _dbContext.Posts.AsNoTracking().Count(t=>t.ForumId == forumid && t.Status <2);
                replycount = _dbContext.Replies.AsNoTracking().Count(r=>r.ForumId == forumid && r.Status <2);
            }
            catch (Exception e)
            {
                _logger.Error("UpdateLastPost: Error fetching last topic", e);
            }

            var forum = new Forum
            {
                Id = forumid
            };// Get(forumid);

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
                _logger.Error("UpdateLastPost: Error updating last post in forum", e);

            }

            return forum;
          
        }

        public PagedList<Post> FetchMyForumTopicsPaged(int pagesize, int pagenum, IEnumerable<int> forumids)
        {
            var result = _dbContext.Posts.AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Forum)
                .Include(p => p.Member).Include(p => p.LastPostAuthor)
                .Where(p=> EF.Constant(forumids).Contains(p.ForumId))
                .Where(p=>p.Status < 2)
                .OrderByDescending(p=>p.LastPostDate);
            return new PagedList<Post>(result, pagenum, pagesize);
        }

        public IEnumerable<string> GetTagStrings(List<int> list)
        {
            return _dbContext.Posts.AsNoTracking().Where(f => EF.Constant(list).Contains(f.ForumId)).Select(p => p.Content);

        }
        public IEnumerable<MyViewTopic> FetchAllMyForumTopics(IEnumerable<int> forumids)
        {
            var result = _dbContext.Posts.AsNoTracking()
                .Include(p => p.Forum)
                .Where(p=> EF.Constant(forumids).Contains(p.ForumId))
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
            catch (Exception)
            {

                return 0;
            }
        }

        public Dictionary<int, string> AllowedUsers(int id)
        {
            return CacheProvider.GetOrCreate("AllowedUsers_" + id, () =>  _dbContext.ForumAllowedMembers.AsNoTracking().Include(am => am.Member).Where(am => am.ForumId == id)
                .ToDictionary(u => u.MemberId, u => u.Member?.Name!),TimeSpan.FromHours(1));

        }

        public async Task DeleteArchivedTopics(int id)
        {
            await _dbContext.ArchivedTopics.Where(p => p.ForumId == id).Include(t => t.Replies).ExecuteDeleteAsync();
            var forum = await _dbContext.Forums.FindAsync(id);
            if (forum != null)
            {
                forum.ArchivedTopics = 0;
                forum.ArchivedCount = 0;
                _dbContext.Forums.Update(forum);
                await _dbContext.SaveChangesAsync();
            }
        }

        public IEnumerable<ArchivedPost>? ArchivedPosts(int id)
        {
            return _dbContext.ArchivedTopics.AsNoTracking().Include(p=>p.Forum).Where(f=>f.ForumId == id).AsEnumerable();
        }
    }
}
