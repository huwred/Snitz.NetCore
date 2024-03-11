using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using System.Reflection;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class MVC : Migration
    {
        public string _forumTablePrefix;
        public string _memberTablePrefix;
        private void SetParameters()
        {
            var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var builder = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
            var config = builder.Build();

            _forumTablePrefix = config.GetSection("SnitzForums").GetSection("forumTablePrefix").Value;
            _memberTablePrefix = config.GetSection("SnitzForums").GetSection("memberTablePrefix").Value;

        }
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            SetParameters();
            migrationBuilder.AddColumn<int>(
                name: "T_ALLOW_RATING",
                table: $"FORUM_TOPICS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<short>(
                name: "T_ISPOLL",
                table: $"FORUM_TOPICS",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "T_POLLSTATUS",
                table: $"FORUM_TOPICS",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<int>(
                name: "T_RATING_TOTAL",
                table: $"FORUM_TOPICS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "T_RATING_TOTAL_COUNT",
                table: $"FORUM_TOPICS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "R_RATING",
                table: $"FORUM_REPLY",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "M_LASTACTIVITY",
                table: $"FORUM_MEMBERS",
                type: "nvarchar(14)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "F_POLLS",
                table: $"FORUM_FORUM",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "F_POSTAUTH",
                table: $"FORUM_FORUM",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<short>(
                name: "F_RATING",
                table: $"FORUM_FORUM",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<int>(
                name: "F_REPLYAUTH",
                table: $"FORUM_FORUM",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "T_ALLOW_RATING",
                table: $"FORUM_A_TOPICS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<short>(
                name: "T_ISPOLL",
                table: $"FORUM_A_TOPICS",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "T_POLLSTATUS",
                table: $"FORUM_A_TOPICS",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<int>(
                name: "T_RATING_TOTAL",
                table: $"FORUM_A_TOPICS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "T_RATING_TOTAL_COUNT",
                table: $"FORUM_A_TOPICS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "R_RATING",
                table: $"FORUM_A_REPLY",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: $"FORUM_FORUM",
                keyColumn: "FORUM_ID",
                keyValue: 1,
                columns: new[] { "F_POLLS", "F_POSTAUTH", "F_RATING", "F_REPLYAUTH" },
                values: new object[] { 0, 0, (short)0, 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            SetParameters();
            migrationBuilder.DropColumn(
                name: "T_ALLOW_RATING",
                table: $"FORUM_TOPICS");

            migrationBuilder.DropColumn(
                name: "T_ISPOLL",
                table: $"FORUM_TOPICS");

            migrationBuilder.DropColumn(
                name: "T_POLLSTATUS",
                table: $"FORUM_TOPICS");

            migrationBuilder.DropColumn(
                name: "T_RATING_TOTAL",
                table: $"FORUM_TOPICS");

            migrationBuilder.DropColumn(
                name: "T_RATING_TOTAL_COUNT",
                table: $"FORUM_TOPICS");

            migrationBuilder.DropColumn(
                name: "R_RATING",
                table: $"FORUM_REPLY");

            migrationBuilder.DropColumn(
                name: "M_LASTACTIVITY",
                table: $"FORUM_MEMBERS");

            migrationBuilder.DropColumn(
                name: "F_POLLS",
                table: $"FORUM_FORUM");

            migrationBuilder.DropColumn(
                name: "F_POSTAUTH",
                table: $"FORUM_FORUM");

            migrationBuilder.DropColumn(
                name: "F_RATING",
                table: $"FORUM_FORUM");

            migrationBuilder.DropColumn(
                name: "F_REPLYAUTH",
                table: $"FORUM_FORUM");

            migrationBuilder.DropColumn(
                name: "T_ALLOW_RATING",
                table: $"FORUM_A_TOPICS");

            migrationBuilder.DropColumn(
                name: "T_ISPOLL",
                table: $"FORUM_A_TOPICS");

            migrationBuilder.DropColumn(
                name: "T_POLLSTATUS",
                table: $"FORUM_A_TOPICS");

            migrationBuilder.DropColumn(
                name: "T_RATING_TOTAL",
                table: $"FORUM_A_TOPICS");

            migrationBuilder.DropColumn(
                name: "T_RATING_TOTAL_COUNT",
                table: $"FORUM_A_TOPICS");

            migrationBuilder.DropColumn(
                name: "R_RATING",
                table: $"FORUM_A_REPLY");
        }
    }
}
