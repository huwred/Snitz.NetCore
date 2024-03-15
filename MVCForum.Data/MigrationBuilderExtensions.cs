using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;

namespace SnitzCore.Data
{
    public static class MigrationBuilderExtensions
    {
        public static bool TableExists(this MigrationBuilder migrationBuilder, string table)
        {
            var connstring = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["SnitzConnection"];

            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = connstring;
                conn.Open();
  
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT COUNT(*) FROM sys.tables WHERE name = '{table}'";
  
                int count = (int)cmd.ExecuteScalar();
                conn.Close();
                return count > 0;
            }
        }
        public static bool ColumnExists(this MigrationBuilder migrationBuilder, string tableName, string column)
        {
            var connstring = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["SnitzConnection"];

            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = connstring;
                conn.Open();
  
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT COUNT(*) FROM sys.columns WHERE Name = N'{column}' AND Object_ID = Object_ID(N'{tableName}')";
  
                int count = (int)cmd.ExecuteScalar();
                conn.Close();
                return count > 0;
            }

        }
        public static bool IndexExists(this MigrationBuilder migrationBuilder,string query)
        {
            var connstring = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["SnitzConnection"];

            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = connstring;
                conn.Open();
  
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = query;
                int count = (int)cmd.ExecuteScalar();
                conn.Close();
                return count > 0;
            }

        }
    }
}
