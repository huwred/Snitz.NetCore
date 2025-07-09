using Microsoft.EntityFrameworkCore;
using SnitzCore.Data;
using NetCore.AutoRegisterDi;
using SnitzCore.Data.Interfaces;
using Snitz.PostThanks.Models;

namespace Snitz.PostThanks.Helpers
{
    [RegisterAsScoped]
    public class PostThanksService : ISnitzStartupService
    {
        private readonly PostThanksContext _dbContext;
        private readonly SnitzDbContext _snitzDbContext;

        public PostThanksService(PostThanksContext dbContext, SnitzDbContext snitzDbContext)
        {
            _dbContext = dbContext;
            _snitzDbContext = snitzDbContext;
        }
        public Task<bool> AddItemAsync(object item)
        {
            throw new NotImplementedException();
        }

        public int AuthLevel(int forumid)
        {
            throw new NotImplementedException();
        }

        public bool EnabledForTopic(int topicid)
        {
                var test =  _snitzDbContext.Forums
                .FromSqlInterpolated($"SELECT * FROM FORUM_FORUM WHERE F_ALLOWTHANKS=1");
            return test.Any(f=>f.Id == topicid);
        }

        public IEnumerable<object> Get(int id)
        {
            return _dbContext.PostThanks.AsNoTracking().Where(e=>e.TopicId == id).AsEnumerable();
        }
    }
}
