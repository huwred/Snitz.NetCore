using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Threading.Tasks;

namespace SnitzCore.Service
{
    public static class DbContextsExtensions
    {
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
