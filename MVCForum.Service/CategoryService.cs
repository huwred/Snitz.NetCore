using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace SnitzCore.Service
{
    public class CategoryService : ICategory
    {
        private readonly SnitzDbContext _dbContext;

        public CategoryService(SnitzDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public Category? GetById(int id)
        {
            var category = _dbContext.Categories
                .Include(f => f.Forums)
                .SingleOrDefault(f => f.Id == id);

            return category;
        }

        //[OutputCache(Duration = 600)]
        public IEnumerable<Category> GetAll()
        {
            var result = _dbContext.Categories.AsEnumerable();
            return result;
        }

        public async Task Create(Category category)
        {
            _dbContext.Categories.Add(category);
            await _dbContext.SaveChangesAsync();

        }

        public async Task Delete(int categoryId)
        {
            try
            {
                await _dbContext.Categories.Include(f=>f.Forums).Where(f => f.Id == categoryId).ExecuteDeleteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        public async Task Update(Category category)
        {
            try
            {
                var myObj =  _dbContext.Categories.Find(category.Id);
                if (myObj != null)
                {
                    myObj.Name = category.Name;
                    myObj.Moderation = category.Moderation;
                    myObj.Status = category.Status;
                    myObj.Sort = category.Sort;
                    myObj.Subscription = category.Subscription;
                    _dbContext.Update(myObj);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            await _dbContext.SaveChangesAsync();

        }

        public IEnumerable<Group> GetGroups()
        {
            return _dbContext.Groups.Include(g=>g.GroupName).Include(g=>g.Category).AsQueryable();
        }

        public IEnumerable<GroupName> GetGroupNames()
        {
            return _dbContext.GroupName.AsQueryable();
        }

        public async Task DeleteForums(int id)
        {
            try
            {
                await _dbContext.Posts.Where(f=>f.CategoryId == id).Include(t=>t.Replies).ExecuteDeleteAsync();
                await _dbContext.Forums.Where(f=>f.CategoryId == id).ExecuteDeleteAsync();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        public IEnumerable<Category> FetchCategoryForumList(IPrincipal user)
        {
            var categories = from a in _dbContext.Categories
             join b in _dbContext.Forums on a.Id equals b.CategoryId 
             join c in _dbContext.Posts on b.LatestTopicId equals c.Id 
             join d in _dbContext.Members on c.MemberId equals d.Id into joinedData
             from d in joinedData.DefaultIfEmpty()
             select new Category()
             {
                Id = a.Id,
                Name = a.Name,
                Moderation = a.Moderation,
                Status = a.Status,
                Sort = a.Sort,
                Forums = new List<Forum>()
                {
                    new Forum()
                    {
                        Id = b.Id,
                        Title = b.Title,
                        Description = b.Description,
                        LatestTopic = new Post()
                        {
                            Id = c.Id,
                            Member = d,
                            Created = c.Created
                        },
                        Type = b.Type,
                        Status = b.Status,
                        Moderation = b.Moderation,
                        Subscription = b.Subscription,
                        Privateforums = b.Privateforums,
                        Order = b.Order
                    }
                }
             };

            //var categories = _dbContext.Categories
            //    .Include(f => f.Forums)
            //    .ThenInclude(f => f.LatestTopic)
            //    .ThenInclude(p => p.Member)
            //    .OrderBy(c => c.Sort).ThenBy(f => f.Forums.OrderBy(c => c.Order).ToList()).ToList();
            //    //.AsEnumerable();

            if (user.IsInRole("Administrator"))
            {
                return categories;
            }
            return categories; //.Where(c => c.Forums.Any(f => f.Group.Members.Any(m => m.UserName == user.Identity?.Name)));
        }
    }
}
