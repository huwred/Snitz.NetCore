using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using System;
using System.IO;
using System.Linq;

namespace SnitzCore.Service.Hangfire
{
    public static class Log4NetCleanup
    {
        public static IApplicationBuilder CreateJob(this IApplicationBuilder app,IConfigurationSection config, string contentRootPath)
        {
            var time = config.GetValue<int>("Log4NetCleanupTime",3);
            var olderthan = config.GetValue<int>("Log4NetCleanupOlderThan",14);

            RecurringJob.AddOrUpdate(
            "log4net-cleanup-job", // Job ID
            () => ExecuteTask(contentRootPath,olderthan), // Method to execute
            Cron.Daily(time)); // Cron expression for scheduling (e.g., daily)
            return app;
        }
        public static void ExecuteTask(string contentRootPath, int daysToKeep)
        {
            string logDirectory = Path.Combine(contentRootPath,"Logs"); // Path to your log directory

            foreach (var file in Directory.GetFiles(logDirectory))
            {
                try
                {
                    if (File.GetLastWriteTime(file) < DateTime.Now.AddDays(-daysToKeep))
                    {
                        File.Delete(file);
                    }
                }
                catch (Exception)
                {
                    //supress any errors
                }
            }

        }
    }

    public static class TempForumCleanup
    {
        public static IApplicationBuilder CreateTempForumCleanupJob(this IApplicationBuilder app,IConfigurationSection config, SnitzDbContext context)
        {
            var id = config.GetValue<int>("TempForumId",0);
            if(id == 0)
                return app;

            RecurringJob.AddOrUpdate(
            "temp-forum-cleanup-job", // Job ID
            () => ExecuteTask(context,id), // Method to execute
            Cron.Daily(3)); // Cron expression for scheduling (e.g., daily)
            return app;
        }
        public static void ExecuteTask(SnitzDbContext context, int forumid)
        {
            var forum = context.Forums.Include(p => p.Posts)
                .SingleOrDefault(p => p.Id == forumid);

                foreach (var child in forum.Posts.Where(p => string.Compare( p.Created , DateTime.UtcNow.AddDays(-14).ToForumDateStr()) < 0).ToList())
                    context.Posts.Remove(child);
            context.SaveChanges();

        }
    }
}
