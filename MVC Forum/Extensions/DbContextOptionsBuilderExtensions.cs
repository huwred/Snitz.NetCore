using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace MVCForum.Extensions
{
    public static class DbContextOptionsBuilderExtensions
    {
        const string Mssql = "mssql";
        const string Sqlite = "sqlite";
        const string Npgsql = "npgsql";

        public static DbContextOptionsBuilder UseDatabase(this DbContextOptionsBuilder builder, string? provider, IConfiguration configuration,string? path)
        {
            ArgumentException.ThrowIfNullOrEmpty(nameof(provider), provider);
            provider = provider?.ToLower();
            var test = configuration.GetConnectionString("SnitzConnection");
            return provider switch
            {
                Mssql => builder.UseSqlServer(configuration.GetConnectionString("SnitzConnection"), opt => 
                {
                    opt.MigrationsAssembly("MSSqlMigrations");
                }),

                Sqlite => builder.UseSqlite(configuration.GetConnectionString("SnitzConnection")?.Replace("|DataDirectory|",path), opt => 
                {
                    opt.MigrationsAssembly("SqliteMigrations");
                }),

                //Npgsql => builder.UseNpgsql(configuration.GetConnectionString("SnitzConnection"), opt => 
                //{
                //    opt.MigrationsAssembly("NpgsqlMigrations");
                //}),

                _ => throw new InvalidOperationException($"Unsupported provider: {provider}")
            };
        }
    }
}
