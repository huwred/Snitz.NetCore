using Hangfire;
using Hangfire.SQLite;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnitzCore.Data;
using SnitzCore.Data.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace SnitzCore.Service.Extensions
{
    public static class DbContextsExtensions
    {
        //private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType!);
        public static IGlobalConfiguration SetStorage(
            this IGlobalConfiguration configuration,
            string nameOrConnectionString, string dbProvider)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (nameOrConnectionString == null) throw new ArgumentNullException(nameof(nameOrConnectionString));

            switch (dbProvider)
            {
                case "sqlite":

                    return configuration.UseStorage(new SQLiteStorage(nameOrConnectionString));
                default:

                    return configuration.UseStorage(new SqlServerStorage(nameOrConnectionString));
            }

        }
        public static async Task<bool> TableExists(this DbContext context, string tableName)
        {
            var connection = context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var command = connection.CreateCommand();
            switch (context.Database.ProviderName)
            {
                case "Microsoft.EntityFrameworkCore.Sqlite" :
                    command.CommandText = $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{tableName}';";
                    break;
                case "Pomelo.EntityFrameworkCore.MySql":
                    command.CommandText = $"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = '{tableName}';";
                    break;
                default:
                    command.CommandText = $"SELECT COUNT(*) FROM sys.tables WHERE name = '{tableName}'";
                    break;
            }
            
            var result = await command.ExecuteScalarAsync();
            if (result != null)
            {
                var count = int.Parse(result.ToString()!);
                return count > 0;
            }
            return false;
        }

        public static void ImportLangResCSV(this SnitzDbContext context, string path, bool updateExisting)
        {
            log4net.ILog _logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType!);
            var csv = new CSVFiles(path, new[]
            {
                new DataColumn("pk", typeof(string)),
                new DataColumn("ResourceId", typeof(string)),
                new DataColumn("Value", typeof(string)),
                new DataColumn("Culture", typeof(string)),
                new DataColumn("Type", typeof(string)),
                new DataColumn("ResourceSet", typeof(string))
            });

            DataTable dt = csv.Table;

            try
            {
                using var transaction = context.Database.BeginTransaction();
                foreach (DataRow row in dt.Rows)
                {
                    if (String.IsNullOrWhiteSpace(row.Field<string>("ResourceId")))
                        continue;
                    LanguageResource res = new LanguageResource
                    {
                        Name = row.Field<string>("ResourceId")!,
                        ResourceSet = row.Field<string>("ResourceSet"),
                        Value = row.Field<string>("Value"),
                        Culture = row.Field<string>("Culture")!
                    };

                    var existing = context.LanguageResources.SingleOrDefault(l=>l.Culture == res.Culture && l.Name == res.Name);

                    if (existing != null && !existing.Equals(default(LanguageResource)))
                    {
                        if (updateExisting && res.ResourceSet !=null)
                        {
                            existing.Value = res.Value;
                            existing.ResourceSet = res.ResourceSet;
                            context.LanguageResources.Update(existing);
                        }
                    }
                    else
                    {
                        context.LanguageResources.Add(res);
                    }
                    context.SaveChanges();
                }
                transaction.Commit();
            }
            catch (Exception e)
            {
                _logger.Error("Error importing language resources from CSV file.",e);
            }

        }
    }
}
