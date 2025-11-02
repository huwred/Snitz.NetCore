using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

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
}
