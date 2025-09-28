using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Models;
using SnitzCore.Service.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SnitzCore.Service
{
    public class ArchiveService
    {
        IServiceProvider _serviceProvider;
        private readonly string? _tableprefix;
        private readonly log4net.ILog _logger;

        public ArchiveService(IServiceProvider serviceProvider, IOptions<SnitzForums> config)
        {
            _serviceProvider = serviceProvider;
            _tableprefix = config.Value.forumTablePrefix;
            _logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType!);
        }


        public async Task ArchiveTopics(int forumId, string? archiveDate)
        {
            IEnumerable<int> topics;
            using (IServiceScope scope = _serviceProvider.CreateScope())
            using (SnitzDbContext _dbContext = scope.ServiceProvider.GetRequiredService<SnitzDbContext>())
            {
                if (!string.IsNullOrWhiteSpace(archiveDate))
                {
                    topics = _dbContext.Posts.AsNoTracking().Where(t => t.ForumId == forumId && string.Compare(t.LastPostDate, archiveDate) < 0).Select(t => t.Id);
                }
                else
                {
                    topics = _dbContext.Posts.AsNoTracking().Where(t => t.ForumId == forumId).Select(t => t.Id);
                }
                if (topics.Any())
                {
                    try
                    {
                        var topiclist = string.Join(",", topics);
                        var sql =
                            @$"INSERT INTO {_tableprefix}A_REPLY (CAT_ID,FORUM_ID,TOPIC_ID,REPLY_ID,R_MAIL,R_AUTHOR,R_MESSAGE,R_DATE,R_IP,R_STATUS,R_LAST_EDIT,R_LAST_EDITBY,R_SIG,R_RATING)
                        SELECT CAT_ID,FORUM_ID,TOPIC_ID,REPLY_ID,R_MAIL,R_AUTHOR,R_MESSAGE,R_DATE,R_IP,R_STATUS,R_LAST_EDIT,R_LAST_EDITBY,R_SIG,R_RATING FROM {_tableprefix}REPLY WHERE TOPIC_ID IN ({topiclist});
                        INSERT INTO {_tableprefix}A_TOPICS (CAT_ID,FORUM_ID,TOPIC_ID,T_STATUS,T_MAIL,T_SUBJECT,T_MESSAGE,T_AUTHOR,T_REPLIES,T_UREPLIES,T_VIEW_COUNT,T_LAST_POST,T_DATE,T_LAST_POSTER,T_IP,T_LAST_POST_AUTHOR,T_LAST_POST_REPLY_ID,T_LAST_EDIT,T_LAST_EDITBY,T_STICKY,T_SIG)
                        SELECT CAT_ID,FORUM_ID,TOPIC_ID,T_STATUS,T_MAIL,T_SUBJECT,T_MESSAGE,T_AUTHOR,T_REPLIES,T_UREPLIES,T_VIEW_COUNT,T_LAST_POST,T_DATE,T_LAST_POSTER,T_IP,T_LAST_POST_AUTHOR,T_LAST_POST_REPLY_ID,T_LAST_EDIT,T_LAST_EDITBY,T_STICKY,T_SIG FROM {_tableprefix}TOPICS WHERE TOPIC_ID IN ({topiclist});
                        DELETE FROM {_tableprefix}REPLY WHERE TOPIC_ID IN ({topiclist}) ;
                        DELETE FROM {_tableprefix}TOPICS WHERE TOPIC_ID IN ({topiclist});
                        UPDATE {_tableprefix}FORUM SET F_L_ARCHIVE={DateTime.UtcNow.ToForumDateStr()} WHERE FORUM_ID={forumId}
                        UPDATE {_tableprefix}TOTALS SET T_A_COUNT = (SELECT COUNT(TOPIC_ID) FROM {_tableprefix}A_TOPICS), P_A_COUNT = (SELECT COUNT(REPLY_ID) FROM {_tableprefix}A_REPLY)";

                        var fs = FormattableStringFactory.Create(sql);
                        _dbContext.Database.BeginTransaction();
                        _dbContext.Database.ExecuteSql(fs);
                        _dbContext.Database.CommitTransaction();
                        await _dbContext.Database.ExecuteSqlRawAsync(
                            "EXEC snitz_updateCounts "
                        );
                    }
                    catch (Exception e)
                    {
                        _logger.Error("ArchiveTopics: Error archiving topics", e);
                        _dbContext.Database.RollbackTransaction();
                    }
                    finally
                    {
                        CacheProvider.Clear();
                    }

                }
            }
        }

        public void DeleteArchivedTopics(int id)
        {
            using (IServiceScope scope = _serviceProvider.CreateScope())
            using (SnitzDbContext _dbContext = scope.ServiceProvider.GetRequiredService<SnitzDbContext>())
            {
                //
                _dbContext.ArchivedTopics.Include(t => t.ArchivedReplies).Where(p => p.ForumId == id).ExecuteDelete();
                var forum = _dbContext.Forums.Find(id);
                try
                {
                    if (forum != null)
                    {
                        forum.ArchivedTopics = 0;
                        forum.ArchivedCount = 0;
                        forum.LastDelete = DateTime.UtcNow.ToForumDateStr();
                        _dbContext.Update(forum);

                        _dbContext.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    _logger.Error("DeleteArchivedTopics: Error deleting archived topics", e);
                }
            }
        }
    }
}