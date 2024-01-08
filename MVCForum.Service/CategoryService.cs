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
            var category = _dbContext.Categories.Include(f=>f.Forums).SingleOrDefault(f => f.Id == categoryId);
            if (category != null) _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync();
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
    }
}
