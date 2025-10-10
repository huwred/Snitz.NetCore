using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SnitzCore.Data.Models
{
    public partial class SnitzMigration : Migration
    {
        public string _forumTablePrefix;
        public string _memberTablePrefix;

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //throw new NotImplementedException();
        }

        protected void SetParameters()
        {
            var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var builder = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) 
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
            var config = builder.Build();

            _forumTablePrefix = config.GetSection("SnitzForums").GetSection("forumTablePrefix").Value;
            _memberTablePrefix = config.GetSection("SnitzForums").GetSection("memberTablePrefix").Value;

        }
    }
}
