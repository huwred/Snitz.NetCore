using System;
using Microsoft.EntityFrameworkCore;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System.Collections.Generic;
using System.Linq;
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
        public Category GetById(int id)
        {
            var category = _dbContext.Categories.Where(f => f.Id == id)
                .Include(f => f.Forums)
                .First();

            return category;
        }

        public IEnumerable<Category> GetAll()
        {
            var result = _dbContext.Categories.Include(forum => forum.Forums)
            .ThenInclude(f=>f.Posts);
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
            _dbContext.Update(category);
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
    }
}
