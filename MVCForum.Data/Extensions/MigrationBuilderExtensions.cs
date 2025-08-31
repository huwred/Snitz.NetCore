using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SnitzCore.Data.Extensions
{
    public static class MigrationBuilderExtensions
    {
        //E:\GitHub\Snitz.NetCore\MVC Forum\App_Data
        public static bool TableExists(this MigrationBuilder migrationBuilder, string table)
        {
            var connstring = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["SnitzConnection"];
             
            if (migrationBuilder.IsSqlite())
            {
                var path = Path.Combine(Regex.Replace(AppDomain.CurrentDomain.BaseDirectory, @"\\bin\\.*$", string.Empty, RegexOptions.IgnoreCase) ,  "App_Data");
                using var conn = new SqliteConnection();
                conn.ConnectionString = connstring?.Replace("|DataDirectory|",path);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{table}';";
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                conn.Close();
                return count > 0;
            }else if (migrationBuilder.IsSqlServer())
            {
                using var conn = new SqlConnection();
                conn.ConnectionString = connstring;
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{table}'";
                var count = cmd.ExecuteScalar();
                conn.Close();
                return count != null ? (int)count > 0 : false;
            }
            return false;
        }
        public static bool ColumnExists(this MigrationBuilder migrationBuilder, string tableName, string column)
        {
            var connstring = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["SnitzConnection"];

            if (migrationBuilder.IsSqlite())
            {
                var path = Path.Combine(Regex.Replace(AppDomain.CurrentDomain.BaseDirectory, @"\\bin\\.*$", string.Empty, RegexOptions.IgnoreCase) ,  "App_Data");
                using var conn = new SqliteConnection();
                conn.ConnectionString = connstring?.Replace("|DataDirectory|",path);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"PRAGMA table_info({tableName});";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (reader["name"].ToString() == column)
                    {
                        conn.Close();
                        return true;
                    }
                }
                conn.Close();
                return false;
            }
            else if (migrationBuilder.IsSqlServer())
            {
                using var conn = new SqlConnection();
                conn.ConnectionString = connstring;
                conn.Open();

                using var cmd = conn.CreateCommand();;
                cmd.CommandText = $"SELECT COUNT(*) FROM sys.columns WHERE Name = N'{column}' AND Object_ID = Object_ID(N'{tableName}')";

                int count = (int)cmd.ExecuteScalar();
                conn.Close();
                return count > 0;
            }
            return false;
        }
        public static bool IndexExists(this MigrationBuilder migrationBuilder, string query)
        {
            var connstring = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["SnitzConnection"];

            if (migrationBuilder.IsSqlite())
            {
                var path = Path.Combine( Regex.Replace(AppDomain.CurrentDomain.BaseDirectory, @"\\bin\\.*$", string.Empty, RegexOptions.IgnoreCase) ,  "App_Data");
                using var conn = new SqliteConnection();
                conn.ConnectionString = connstring?.Replace("|DataDirectory|",path);
                conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = query;
                var count = cmd.ExecuteScalar();
                conn.Close();
                return count != null && (int)count > 0;
            }
            else if (migrationBuilder.IsSqlServer())
            {
                using var conn = new SqlConnection();
                conn.ConnectionString = connstring;
                conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = query;
                int count = (int)cmd.ExecuteScalar();
                conn.Close();
                return count > 0;
            }
            return false;
        }
    }
}
