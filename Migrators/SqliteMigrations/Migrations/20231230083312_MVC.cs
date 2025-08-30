using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using SnitzCore.Data.Models;
using System.Reflection;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class MVC : SnitzMigration
    {

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            SetParameters();
                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}RATINGS",
                    columns: table => new
                    {
                        RATING = table.Column<int>(type: "INTEGER", nullable: false)
                            .Annotation("Sqlite:Autoincrement", true),
                        RATINGS_BYMEMBER_ID = table.Column<int>(type: "INTEGER", nullable: false),
                        RATINGS_TOPIC_ID = table.Column<int>(type: "INTEGER", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_TOPIC_RATINGS", x => x.RATING);
                    });

            migrationBuilder.AddColumn<int>(
                name: "T_ALLOW_RATING",
                table: $"{_forumTablePrefix}TOPICS",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<short>(
                name: "T_ISPOLL",
                table: $"{_forumTablePrefix}TOPICS",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "T_POLLSTATUS",
                table: $"{_forumTablePrefix}TOPICS",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<int>(
                name: "T_RATING_TOTAL",
                table: $"{_forumTablePrefix}TOPICS",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "T_RATING_TOTAL_COUNT",
                table: $"{_forumTablePrefix}TOPICS",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "R_RATING",
                table: $"{_forumTablePrefix}REPLY",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "M_LASTACTIVITY",
                table: $"{_memberTablePrefix}MEMBERS",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "F_POLLS",
                table: $"{_forumTablePrefix}FORUM",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "F_POSTAUTH",
                table: $"{_forumTablePrefix}FORUM",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<short>(
                name: "F_RATING",
                table: $"{_forumTablePrefix}FORUM",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<int>(
                name: "F_REPLYAUTH",
                table: $"{_forumTablePrefix}FORUM",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "T_ALLOW_RATING",
                table: $"{_forumTablePrefix}A_TOPICS",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<short>(
                name: "T_ISPOLL",
                table: $"{_forumTablePrefix}A_TOPICS",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "T_POLLSTATUS",
                table: $"{_forumTablePrefix}A_TOPICS",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<int>(
                name: "T_RATING_TOTAL",
                table: $"{_forumTablePrefix}A_TOPICS",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "T_RATING_TOTAL_COUNT",
                table: $"{_forumTablePrefix}A_TOPICS",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "R_RATING",
                table: $"{_forumTablePrefix}A_REPLY",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: $"{_forumTablePrefix}FORUM",
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
                table: $"{_forumTablePrefix}TOPICS");

            migrationBuilder.DropColumn(
                name: "T_ISPOLL",
                table: $"{_forumTablePrefix}TOPICS");

            migrationBuilder.DropColumn(
                name: "T_POLLSTATUS",
                table: $"{_forumTablePrefix}TOPICS");

            migrationBuilder.DropColumn(
                name: "T_RATING_TOTAL",
                table: $"{_forumTablePrefix}TOPICS");

            migrationBuilder.DropColumn(
                name: "T_RATING_TOTAL_COUNT",
                table: $"{_forumTablePrefix}TOPICS");

            migrationBuilder.DropColumn(
                name: "R_RATING",
                table: $"{_forumTablePrefix}REPLY");

            migrationBuilder.DropColumn(
                name: "M_LASTACTIVITY",
                table: $"{_memberTablePrefix}MEMBERS");

            migrationBuilder.DropColumn(
                name: "F_POLLS",
                table: $"{_forumTablePrefix}FORUM");

            migrationBuilder.DropColumn(
                name: "F_POSTAUTH",
                table: $"{_forumTablePrefix}FORUM");

            migrationBuilder.DropColumn(
                name: "F_RATING",
                table: $"{_forumTablePrefix}FORUM");

            migrationBuilder.DropColumn(
                name: "F_REPLYAUTH",
                table: $"{_forumTablePrefix}FORUM");

            migrationBuilder.DropColumn(
                name: "T_ALLOW_RATING",
                table: $"{_forumTablePrefix}A_TOPICS");

            migrationBuilder.DropColumn(
                name: "T_ISPOLL",
                table: $"{_forumTablePrefix}A_TOPICS");

            migrationBuilder.DropColumn(
                name: "T_POLLSTATUS",
                table: $"{_forumTablePrefix}A_TOPICS");

            migrationBuilder.DropColumn(
                name: "T_RATING_TOTAL",
                table: $"{_forumTablePrefix}A_TOPICS");

            migrationBuilder.DropColumn(
                name: "T_RATING_TOTAL_COUNT",
                table: $"{_forumTablePrefix}A_TOPICS");

            migrationBuilder.DropColumn(
                name: "R_RATING",
                table: $"{_forumTablePrefix}A_REPLY");
        }
    }
}
