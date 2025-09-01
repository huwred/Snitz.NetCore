using Microsoft.EntityFrameworkCore;
using NetCore.AutoRegisterDi;
using Snitz.Events.Models;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;

namespace Snitz.Events.Helpers
{
    //[RegisterAsScoped]
    public class CalEventsService : ISnitzStartupService
    {
        //private readonly EventContext _dbContext;
        private readonly SnitzDbContext _snitzDbContext;

        public CalEventsService(/*EventContext dbContext,*/ SnitzDbContext snitzDbContext)
        {
            //_dbContext = dbContext;
            _snitzDbContext = snitzDbContext;
        }
        public bool EnabledForTopic(int topicid)
        {
            //throw new NotImplementedException();
            try
            {
                //    return _dbContext.EventItems.SingleOrDefault(e=>e.TopicId == topicid) != null;
                var auth = _snitzDbContext.Database.SqlQuery<int>(
                    $"SELECT C_ID AS Value FROM CAL_EVENTS WHERE TOPIC_ID={topicid}").Single();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

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
            throw new NotImplementedException();
            //try
            //{
            //    return _dbContext.EventItems.AsNoTracking().Where(e=>e.TopicId == id).AsEnumerable();

            //}
            //catch (Exception)
            //{

            //    throw;
            //}
        }

        public async Task<bool> AddItemAsync(object item)
        {
            return true;
            //try
            //{
            //    _dbContext.EventItems.Add((CalendarEventItem)item);
            //    return await _dbContext.SaveChangesAsync() > 0;
            //}
            //catch (Exception e)
            //{
            //    return false;
            //}
            
        }
    }
}
