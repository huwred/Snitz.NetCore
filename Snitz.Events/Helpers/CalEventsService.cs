using Microsoft.EntityFrameworkCore;
using Snitz.Events.Models;
using SnitzCore.Data;
using NetCore.AutoRegisterDi;
using SnitzCore.Data.Interfaces;

namespace Snitz.Events.Helpers
{
    [RegisterAsScoped]
    public class CalEventsService : ISnitzStartupService
    {
        private readonly EventContext _dbContext;
        private readonly SnitzDbContext _snitzDbContext;

        public CalEventsService(EventContext dbContext, SnitzDbContext snitzDbContext)
        {
            _dbContext = dbContext;
            _snitzDbContext = snitzDbContext;
        }
        public bool EnabledForTopic(int topicid)
        {
            return _dbContext.EventItems.SingleOrDefault(e=>e.TopicId == topicid) != null;

        }

        public int AuthLevel(int forumid)
        {
            try
            {
                var auth = _snitzDbContext.Database.SqlQuery<int>(
                    $"SELECT F_ALLOWEVENTS AS Value FROM FORUM_FORUM WHERE FORUM_ID={forumid}").Single();
                return auth;
            }
            catch (Exception e)
            {
                return 0;
            }

        }

        public IEnumerable<object> Get(int id)
        {
            return _dbContext.EventItems.AsNoTracking().Where(e=>e.TopicId == id).AsEnumerable();
        }

        public async Task<bool> AddItemAsync(object item)
        {
            return true;
            try
            {
                _dbContext.EventItems.Add((CalendarEventItem)item);
                return await _dbContext.SaveChangesAsync() > 0;
            }
            catch (Exception e)
            {
                return false;
            }
            
        }
    }
}
