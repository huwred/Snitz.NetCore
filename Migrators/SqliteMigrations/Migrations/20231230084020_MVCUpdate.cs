using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Models;
using System;
using System.Reflection;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class MVCUpdate : SnitzMigration
    {

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            SetParameters();
            migrationBuilder.AddColumn<int>(
                name: "M_PMEMAIL",
                table: $"{_memberTablePrefix}MEMBERS",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "M_PMRECEIVE",
                table: $"{_memberTablePrefix}MEMBERS",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<short>(
                name: "M_PMSAVESENT",
                table: $"{_memberTablePrefix}MEMBERS",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "M_PRIVATEPROFILE",
                table: $"{_memberTablePrefix}MEMBERS",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);
            if (!migrationBuilder.ColumnExists($"{_memberTablePrefix}MEMBERS", "M_LASTHEREDATE"))
            {
                migrationBuilder.AddColumn<bool>(
                    name: "M_LASTHEREDATE",
                    table: $"{_memberTablePrefix}MEMBERS",
                    type: "TEXT",
                    maxLength: 14,
                    nullable: true);
            }

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}POLLS",
                columns: table => new
                {
                    POLL_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CAT_ID = table.Column<string>(type: "INTEGER", nullable: false),
                    FORUM_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    TOPIC_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    P_WHOVOTES = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    P_LASTVOTE = table.Column<string>(type: "TEXT", maxLength: 14, nullable: true),
                    P_QUESTION = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_POLLS", x => x.POLL_ID);
                });
            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}POLL_ANSWERS",
                columns: table => new
                {
                    POLLANSWER_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    POLL_ID = table.Column<string>(type: "INTEGER", nullable: false),
                    POLLANSWER_ORDER = table.Column<int>(type: "INTEGER", nullable: false),
                    POLLANSWER_COUNT = table.Column<int>(type: "INTEGER", nullable: false),
                    POLLANSWER_LABEL = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_POLL_ANSWERS", x => x.POLLANSWER_ID);
                });
            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}POLL_VOTES",
                columns: table => new
                {
                    POLLVOTES_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    POLL_ID = table.Column<string>(type: "INTEGER", nullable: false),
                    CAT_ID = table.Column<string>(type: "INTEGER", nullable: true),
                    FORUM_ID = table.Column<int>(type: "INTEGER", nullable: true),
                    TOPIC_ID = table.Column<int>(type: "INTEGER", nullable: true),
                    MEMBER_ID = table.Column<int>(type: "INTEGER", nullable: true),
                    GUEST_VOTE = table.Column<int>(type: "INTEGER", nullable: true),

                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_POLL_VOTES", x => x.POLLVOTES_ID);
                });

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}PM",
                columns: table => new
                {
                    M_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    M_SUBJECT = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    M_FROM = table.Column<int>(type: "INTEGER", nullable: false),
                    M_TO = table.Column<int>(type: "INTEGER", nullable: false),
                    M_SENT = table.Column<string>(type: "TEXT", maxLength: 14, nullable: true),
                    M_MESSAGE = table.Column<string>(type: "TEXT", nullable: false),
                    M_PMCOUNT = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    M_READ = table.Column<int>(type: "INTEGER", nullable: false),
                    M_MAIL = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    M_OUTBOX = table.Column<short>(type: "INTEGER", nullable: false),
                    PM_DEL_FROM = table.Column<int>(type: "INTEGER", nullable: false),
                    PM_DEL_TO = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_PM", x => x.M_ID);
                });

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}PM_BLOCKLIST",
                columns: table => new
                {
                    BL_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BL_MEMBER_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    BL_BLOCKED_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    BL_BLOCKED_NAME = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_PM_BLOCKLIST", x => x.BL_ID);
                });

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}RANKING",
                columns: table => new
                {
                    RANK_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    R_TITLE = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    R_IMAGE = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    R_POSTS = table.Column<int>(type: "INTEGER", nullable: true),
                    R_IMG_REPEAT = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_RANKING", x => x.RANK_ID);
                });

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}SPAM_MAIL",
                columns: table => new
                {
                    SPAM_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SPAM_SERVER = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_SPAM_MAIL", x => x.SPAM_ID);
                });

            migrationBuilder.CreateTable(
                name: "LANGUAGE_RES",
                columns: table => new
                {
                    pk = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ResourceId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    Culture = table.Column<string>(type: "TEXT", maxLength: 6, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    ResourceSet = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LANGUAGE_RES", x => x.pk);
                });

            migrationBuilder.CreateTable(
                name: "webpages_Membership",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", maxLength: 75, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_webpages_Membership", x => x.UserId);
                });

            migrationBuilder.UpdateData(
                table: $"{_memberTablePrefix}MEMBERS",
                keyColumn: "MEMBER_ID",
                keyValue: 1,
                columns: new[] { "M_DATE", "M_PMEMAIL", "M_PMRECEIVE", "M_PMSAVESENT", "M_PRIVATEPROFILE" },
                values: new object[] { "20231230084020", 0, 0, (short)0, (short)0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            SetParameters();
            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}PM");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}PM_BLOCKLIST");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}RANKING");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}SPAM_MAIL");

            migrationBuilder.DropTable(
                name: "LANGUAGE_RES");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}RATINGS");

            migrationBuilder.DropTable(
                name: "webpages_Membership");

            migrationBuilder.DropColumn(
                name: "M_PMEMAIL",
                table: $"{_memberTablePrefix}MEMBERS");

            migrationBuilder.DropColumn(
                name: "M_PMRECEIVE",
                table: $"{_memberTablePrefix}MEMBERS");

            migrationBuilder.DropColumn(
                name: "M_PMSAVESENT",
                table: $"{_memberTablePrefix}MEMBERS");

            migrationBuilder.DropColumn(
                name: "M_PRIVATEPROFILE",
                table: $"{_memberTablePrefix}MEMBERS");

            migrationBuilder.UpdateData(
                table: $"{_memberTablePrefix}MEMBERS",
                keyColumn: "MEMBER_ID",
                keyValue: 1,
                column: "M_DATE",
                value: "20231230083312");
        }
    }
}
