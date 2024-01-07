using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using System.Reflection;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class MVCUpdate : Migration
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
                name: "M_PMEMAIL",
                table: $"{_memberTablePrefix}MEMBERS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "M_PMRECEIVE",
                table: $"{_memberTablePrefix}MEMBERS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<short>(
                name: "M_PMSAVESENT",
                table: $"{_memberTablePrefix}MEMBERS",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "M_PRIVATEPROFILE",
                table: $"{_memberTablePrefix}MEMBERS",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}PM",
                columns: table => new
                {
                    M_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    M_SUBJECT = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    M_FROM = table.Column<int>(type: "int", nullable: false),
                    M_TO = table.Column<int>(type: "int", nullable: false),
                    M_SENT = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                    M_MESSAGE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    M_PMCOUNT = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    M_READ = table.Column<int>(type: "int", nullable: false),
                    M_MAIL = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    M_OUTBOX = table.Column<short>(type: "int", nullable: false),
                    PM_DEL_FROM = table.Column<int>(type: "int", nullable: false),
                    PM_DEL_TO = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_PM", x => x.M_ID);
                });

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}PM_BLOCKLIST",
                columns: table => new
                {
                    BL_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BL_MEMBER_ID = table.Column<int>(type: "int", nullable: false),
                    BL_BLOCKED_ID = table.Column<int>(type: "int", nullable: false),
                    BL_BLOCKEDNAME = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_PM_BLOCKLIST", x => x.BL_ID);
                });

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}RANKING",
                columns: table => new
                {
                    RANK_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    R_TITLE = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    R_IMAGE = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    R_POSTS = table.Column<int>(type: "int", nullable: true),
                    R_IMG_REPEAT = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_RANKING", x => x.RANK_ID);
                });

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}SPAM_MAIL",
                columns: table => new
                {
                    SPAM_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SPAM_SERVER = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_SPAM_MAIL", x => x.SPAM_ID);
                });

            migrationBuilder.CreateTable(
                name: "LANGUAGE_RES",
                columns: table => new
                {
                    pk = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResourceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Culture = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    ResourceSet = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LANGUAGE_RES", x => x.pk);
                });

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}RATINGS",
                columns: table => new
                {
                    RATING = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RATINGS_BYMEMBERID = table.Column<int>(type: "int", nullable: false),
                    RATINGS_TOPIC_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TOPIC_RATINGS", x => x.RATING);
                });

            migrationBuilder.CreateTable(
                name: "webpages_Membership",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreateDate = table.Column<DateTime>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(75)", maxLength: 75, nullable: false)
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
