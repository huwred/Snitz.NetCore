using Microsoft.EntityFrameworkCore;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnitzCore.Service
{
    public class GroupService : IGroups
    {
        private readonly SnitzDbContext _dbContext;

        public GroupService(SnitzDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task Add(GroupName group)
        {
            _dbContext.GroupName.Add(group);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Create(Group group)
        {
            _dbContext.Groups.Add(group);
            await _dbContext.SaveChangesAsync();
        }

        public Task Delete(GroupName group)
        {
            _dbContext.GroupName.Where(g=>g.Id == group.Id).ExecuteDelete();
            return Task.CompletedTask;
        }

        public Task DeleteCategory(int groupnameid, int categoryid)
        {
            _dbContext.Groups.Where(g=>g.GroupNameId == groupnameid && g.CategoryId == categoryid).ExecuteDelete();
            return Task.CompletedTask;
        }

        public List<Group> GetAll()
        {
            return _dbContext.Groups.Include(g=>g.GroupName).Include(g=>g.Category).ToList();
        }

        public List<Group> GetGroups(int groupnameid)
        {
            return _dbContext.Groups.Where(g=>g.GroupNameId == groupnameid).ToList();
        }

        public List<GroupName> GetNames()
        {
            return _dbContext.GroupName.Include(g=>g.Groups).ToList();
        }
        public Task Update(Group group)
        {
            throw new NotImplementedException();
        }
    }
}
