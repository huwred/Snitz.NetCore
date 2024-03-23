using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hangfire.SqlServer;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Hangfire.SQLite;

namespace SnitzCore.Service.Extensions
{
    public static class DbContextsExtensions
    {
        public static IQueryable<TSource> DistinctBy<TSource, TKey>  (this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            return source.GroupBy(keySelector).Select(x => x.FirstOrDefault());
        }
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
    }
}
