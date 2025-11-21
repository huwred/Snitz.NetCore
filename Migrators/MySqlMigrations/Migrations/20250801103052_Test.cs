using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Models;

#nullable disable

namespace MySqlMigrations.Migrations
{
    /// <inheritdoc />
    public partial class Test : SnitzMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            SetParameters();

            //Console.WriteLine("Adding .NET Identity Schema");

            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            if (!migrationBuilder.TableExists($"AspNetRoles"))
            {
                migrationBuilder.CreateTable(
                    name: "AspNetRoles",
                    columns: table => new
                    {
                        Id = table.Column<string>(type: "varchar(255)", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        NormalizedName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4")
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                    })
                    .Annotation("MySql:CharSet", "utf8mb4");
                migrationBuilder.CreateTable(
                    name: "AspNetRoleClaims",
                    columns: table => new
                    {
                        Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                        RoleId = table.Column<string>(type: "varchar(255)", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        ClaimType = table.Column<string>(type: "longtext", nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4")
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                        table.ForeignKey(
                            name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                            column: x => x.RoleId,
                            principalTable: "AspNetRoles",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                    })
                    .Annotation("MySql:CharSet", "utf8mb4");
            }

            //Console.WriteLine("Applying MySQL initial migration...");
            if (!migrationBuilder.TableExists($"{_forumTablePrefix}BADWORDS"))
            {
                migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}BADWORDS",
                columns: table => new
                {
                    B_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    B_BADWORD = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    B_REPLACE = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_BADWORDS", x => x.B_ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
            }

            if (!migrationBuilder.TableExists($"{_forumTablePrefix}FORUM"))
            {
                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}CATEGORY",
                    columns: table => new
                    {
                        CAT_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                        CAT_STATUS = table.Column<short>(type: "smallint", nullable: true),
                        CAT_NAME = table.Column<string>(type: "longtext", nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        CAT_MODERATION = table.Column<int>(type: "int", nullable: true),
                        CAT_SUBSCRIPTION = table.Column<int>(type: "int", nullable: true),
                        CAT_ORDER = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_CATEGORY", x => x.CAT_ID);
                    })
                    .Annotation("MySql:CharSet", "utf8mb4");
                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}GROUP_NAMES",
                    columns: table => new
                    {
                        GROUP_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                        GROUP_NAME = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        GROUP_DESCRIPTION = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        GROUP_ICON = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        GROUP_IMAGE = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4")
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_GROUP_NAMES", x => x.GROUP_ID);
                    })
                    .Annotation("MySql:CharSet", "utf8mb4");
                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}NAMEFILTER",
                    columns: table => new
                    {
                        N_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                        N_NAME = table.Column<string>(type: "varchar(75)", maxLength: 75, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4")
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_NAMEFILTER", x => x.N_ID);
                    })
                    .Annotation("MySql:CharSet", "utf8mb4");
                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}SPAM_MAIL",
                    columns: table => new
                    {
                        SPAM_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                        SPAM_SERVER = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4")
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_SPAM_MAIL", x => x.SPAM_ID);
                    })
                    .Annotation("MySql:CharSet", "utf8mb4");
                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}TOTALS",
                    columns: table => new
                    {
                        COUNT_ID = table.Column<short>(type: "smallint", nullable: false)
                            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                        P_COUNT = table.Column<int>(type: "int", nullable: false),
                        P_A_COUNT = table.Column<int>(type: "int", nullable: true),
                        T_COUNT = table.Column<int>(type: "int", nullable: false),
                        T_A_COUNT = table.Column<int>(type: "int", nullable: true),
                        U_COUNT = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_TOTALS", x => x.COUNT_ID);
                    })
                    .Annotation("MySql:CharSet", "utf8mb4");
                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}GROUPS",
                    columns: table => new
                    {
                        GROUP_KEY = table.Column<int>(type: "int", nullable: false)
                            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                        GROUP_ID = table.Column<int>(type: "int", nullable: true),
                        GROUP_CATID = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_GROUPS", x => x.GROUP_KEY);
                        table.ForeignKey(
                            name: "FK_FORUM_GROUPS_FORUM_CATEGORY_GROUP_CATID",
                            column: x => x.GROUP_CATID,
                            principalTable: $"{_forumTablePrefix}CATEGORY",
                            principalColumn: "CAT_ID",
                            onDelete: ReferentialAction.Cascade);
                        table.ForeignKey(
                            name: "FK_FORUM_GROUPS_FORUM_GROUP_NAMES_GROUP_ID",
                            column: x => x.GROUP_ID,
                            principalTable: $"{_forumTablePrefix}GROUP_NAMES",
                            principalColumn: "GROUP_ID");
                    })
                    .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}A_REPLY",
                columns: table => new
                {
                    REPLY_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CAT_ID = table.Column<int>(type: "int", nullable: false),
                    FORUM_ID = table.Column<int>(type: "int", nullable: false),
                    TOPIC_ID = table.Column<int>(type: "int", nullable: false),
                    R_MAIL = table.Column<short>(type: "smallint", nullable: false),
                    R_AUTHOR = table.Column<int>(type: "int", nullable: false),
                    R_MESSAGE = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    R_DATE = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    R_IP = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    R_STATUS = table.Column<short>(type: "smallint", nullable: false),
                    R_LAST_EDIT = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    R_LAST_EDITBY = table.Column<int>(type: "int", nullable: true),
                    R_SIG = table.Column<short>(type: "smallint", nullable: false),
                    R_RATING = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_A_REPLY", x => x.REPLY_ID);
                    table.ForeignKey(
                        name: "FK_FORUM_A_REPLY_FORUM_MEMBERS_R_AUTHOR",
                        column: x => x.R_AUTHOR,
                        principalTable: $"{_memberTablePrefix}MEMBERS",
                        principalColumn: "MEMBER_ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}A_TOPICS",
                columns: table => new
                {
                    TOPIC_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CAT_ID = table.Column<int>(type: "int", nullable: false),
                    FORUM_ID = table.Column<int>(type: "int", nullable: false),
                    T_STATUS = table.Column<short>(type: "smallint", nullable: false),
                    T_MAIL = table.Column<short>(type: "smallint", nullable: false),
                    T_SUBJECT = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    T_MESSAGE = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    T_AUTHOR = table.Column<int>(type: "int", nullable: false),
                    T_REPLIES = table.Column<int>(type: "int", nullable: false),
                    T_UREPLIES = table.Column<int>(type: "int", nullable: false),
                    T_VIEW_COUNT = table.Column<int>(type: "int", nullable: false),
                    T_LAST_POST = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    T_DATE = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    T_IP = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    T_LAST_POST_AUTHOR = table.Column<int>(type: "int", nullable: true),
                    T_LAST_POST_REPLY_ID = table.Column<int>(type: "int", nullable: true),
                    T_ARCHIVE_FLAG = table.Column<int>(type: "int", nullable: true),
                    T_LAST_EDIT = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    T_LAST_EDITBY = table.Column<int>(type: "int", nullable: true),
                    T_STICKY = table.Column<short>(type: "smallint", nullable: false),
                    T_SIG = table.Column<short>(type: "smallint", nullable: false),
                    T_ISPOLL = table.Column<short>(type: "smallint", nullable: false),
                    T_POLLSTATUS = table.Column<short>(type: "smallint", nullable: false),
                    T_RATING_TOTAL_COUNT = table.Column<int>(type: "int", nullable: false),
                    T_RATING_TOTAL = table.Column<int>(type: "int", nullable: false),
                    T_ALLOW_RATING = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_A_TOPICS", x => x.TOPIC_ID);
                    table.ForeignKey(
                        name: "FK_FORUM_A_TOPICS_FORUM_CATEGORY_CAT_ID",
                        column: x => x.CAT_ID,
                        principalTable: $"{_forumTablePrefix}CATEGORY",
                        principalColumn: "CAT_ID",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_FORUM_A_TOPICS_FORUM_MEMBERS_T_AUTHOR",
                        column: x => x.T_AUTHOR,
                        principalTable: $"{_memberTablePrefix}MEMBERS",
                        principalColumn: "MEMBER_ID",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_FORUM_A_TOPICS_FORUM_MEMBERS_T_LAST_POST_AUTHOR",
                        column: x => x.T_LAST_POST_AUTHOR,
                        principalTable: $"{_memberTablePrefix}MEMBERS",
                        principalColumn: "MEMBER_ID",onDelete: ReferentialAction.NoAction);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}ALLOWED_MEMBERS",
                columns: table => new
                {
                    MEMBER_ID = table.Column<int>(type: "int", nullable: false),
                    FORUM_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_ALLOWED_MEMBERS", x => new { x.MEMBER_ID, x.FORUM_ID });
                    table.ForeignKey(
                        name: "FK_FORUM_ALLOWED_MEMBERS_FORUM_MEMBERS_MEMBER_ID",
                        column: x => x.MEMBER_ID,
                        principalTable: $"{_memberTablePrefix}MEMBERS",
                        principalColumn: "MEMBER_ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
                migrationBuilder.CreateTable(
                    name: $"{_memberTablePrefix}MEMBERS",
                    columns: table => new
                    {
                        MEMBER_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                        M_NAME = table.Column<string>(type: "varchar(75)", maxLength: 75, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_EMAIL = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_TITLE = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_LEVEL = table.Column<short>(type: "smallint", nullable: false),
                        M_STATUS = table.Column<short>(type: "smallint", nullable: false),
                        M_POSTS = table.Column<int>(type: "int", nullable: false),
                        M_LASTHEREDATE = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_LASTPOSTDATE = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_DATE = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_FIRSTNAME = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_LASTNAME = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_CITY = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_STATE = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_PHOTO_URL = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_COUNTRY = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_SEX = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_AGE = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_DOB = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_OCCUPATION = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_HOMEPAGE = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_MARSTATUS = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_DEFAULT_VIEW = table.Column<int>(type: "int", nullable: false),
                        M_SIG = table.Column<string>(type: "longtext", nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_VIEW_SIG = table.Column<short>(type: "smallint", nullable: false),
                        M_SIG_DEFAULT = table.Column<short>(type: "smallint", nullable: false),
                        M_HIDE_EMAIL = table.Column<short>(type: "smallint", nullable: false),
                        M_RECEIVE_EMAIL = table.Column<short>(type: "smallint", nullable: false),
                        M_LAST_IP = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_IP = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_ALLOWEMAIL = table.Column<short>(type: "smallint", nullable: false),
                        M_SUBSCRIPTION = table.Column<short>(type: "smallint", nullable: false),
                        M_KEY = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_NEWEMAIL = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_PWKEY = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_SHA256 = table.Column<short>(type: "smallint", nullable: true),
                        M_AIM = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_ICQ = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_MSN = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_YAHOO = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_HOBBIES = table.Column<string>(type: "longtext", nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_LNEWS = table.Column<string>(type: "longtext", nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_QUOTE = table.Column<string>(type: "longtext", nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_BIO = table.Column<string>(type: "longtext", nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_LINK1 = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_LINK2 = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_PMEMAIL = table.Column<int>(type: "int", nullable: true),
                        M_PMRECEIVE = table.Column<int>(type: "int", nullable: true),
                        M_PMSAVESENT = table.Column<short>(type: "smallint", nullable: true),
                        M_PRIVATEPROFILE = table.Column<short>(type: "smallint", nullable: true),
                        M_LASTACTIVITY = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4")
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_MEMBERS", x => x.MEMBER_ID);
                    })
                    .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}FORUM",
                columns: table => new
                {
                    FORUM_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CAT_ID = table.Column<int>(type: "int", nullable: false),
                    F_STATUS = table.Column<short>(type: "smallint", nullable: false),
                    F_MAIL = table.Column<short>(type: "smallint", nullable: false),
                    F_SUBJECT = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    F_URL = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    F_DESCRIPTION = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    F_TOPICS = table.Column<int>(type: "int", nullable: false),
                    F_COUNT = table.Column<int>(type: "int", nullable: false),
                    F_LAST_POST = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    F_PASSWORD_NEW = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    F_PRIVATEFORUMS = table.Column<int>(type: "int", nullable: false),
                    F_TYPE = table.Column<short>(type: "smallint", nullable: false),
                    F_IP = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    F_LAST_POST_AUTHOR = table.Column<int>(type: "int", nullable: true),
                    F_LAST_POST_TOPIC_ID = table.Column<int>(type: "int", nullable: true),
                    F_LAST_POST_REPLY_ID = table.Column<int>(type: "int", nullable: true),
                    F_A_TOPICS = table.Column<int>(type: "int", nullable: true),
                    F_A_COUNT = table.Column<int>(type: "int", nullable: true),
                    F_MODERATION = table.Column<int>(type: "int", nullable: false),
                    F_SUBSCRIPTION = table.Column<int>(type: "int", nullable: false),
                    F_ORDER = table.Column<int>(type: "int", nullable: false),
                    F_DEFAULTDAYS = table.Column<int>(type: "int", nullable: false),
                    F_COUNT_M_POSTS = table.Column<short>(type: "smallint", nullable: false),
                    F_L_ARCHIVE = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    F_ARCHIVE_SCHED = table.Column<int>(type: "int", nullable: false),
                    F_L_DELETE = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    F_DELETE_SCHED = table.Column<int>(type: "int", nullable: false),
                    F_POLLS = table.Column<int>(type: "int", nullable: true),
                    F_RATING = table.Column<short>(type: "smallint", nullable: true),
                    F_POSTAUTH = table.Column<int>(type: "int", nullable: true),
                    F_REPLYAUTH = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_FORUM", x => x.FORUM_ID);
                    table.ForeignKey(
                        name: "FK_FORUM_FORUM_FORUM_CATEGORY_CAT_ID",
                        column: x => x.CAT_ID,
                        principalTable: $"{_forumTablePrefix}CATEGORY",
                        principalColumn: "CAT_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FORUM_FORUM_FORUM_MEMBERS_F_LAST_POST_AUTHOR",
                        column: x => x.F_LAST_POST_AUTHOR,
                        principalTable: $"{_memberTablePrefix}MEMBERS",
                        principalColumn: "MEMBER_ID");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}MODERATOR",
                columns: table => new
                {
                    MOD_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FORUM_ID = table.Column<int>(type: "int", nullable: false),
                    MEMBER_ID = table.Column<int>(type: "int", nullable: false),
                    MOD_TYPE = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_MODERATOR", x => x.MOD_ID);
                    table.ForeignKey(
                        name: "FK_FORUM_MODERATOR_FORUM_FORUM_FORUM_ID",
                        column: x => x.FORUM_ID,
                        principalTable: $"{_forumTablePrefix}FORUM",
                        principalColumn: "FORUM_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FORUM_MODERATOR_FORUM_MEMBERS_MEMBER_ID",
                        column: x => x.MEMBER_ID,
                        principalTable: $"{_memberTablePrefix}MEMBERS",
                        principalColumn: "MEMBER_ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}REPLY",
                columns: table => new
                {
                    REPLY_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CAT_ID = table.Column<int>(type: "int", nullable: false),
                    FORUM_ID = table.Column<int>(type: "int", nullable: false),
                    TOPIC_ID = table.Column<int>(type: "int", nullable: false),
                    R_MAIL = table.Column<short>(type: "smallint", nullable: false),
                    R_AUTHOR = table.Column<int>(type: "int", nullable: false),
                    R_MESSAGE = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    R_DATE = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    R_IP = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    R_STATUS = table.Column<short>(type: "smallint", nullable: false),
                    R_LAST_EDIT = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    R_LAST_EDITBY = table.Column<int>(type: "int", nullable: true),
                    R_SIG = table.Column<short>(type: "smallint", nullable: false),
                    R_RATING = table.Column<int>(type: "int", nullable: false),
                    R_ANSWER = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_REPLY", x => x.REPLY_ID);
                    table.ForeignKey(
                        name: "FK_FORUM_REPLY_FORUM_MEMBERS_R_AUTHOR",
                        column: x => x.R_AUTHOR,
                        principalTable: $"{_memberTablePrefix}MEMBERS",
                        principalColumn: "MEMBER_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FORUM_REPLY_FORUM_TOPICS_TOPIC_ID",
                        column: x => x.TOPIC_ID,
                        principalTable: $"{_forumTablePrefix}TOPICS",
                        principalColumn: "TOPIC_ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}SUBSCRIPTIONS",
                columns: table => new
                {
                    SUBSCRIPTION_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MEMBER_ID = table.Column<int>(type: "int", nullable: false),
                    CAT_ID = table.Column<int>(type: "int", nullable: false),
                    FORUM_ID = table.Column<int>(type: "int", nullable: false),
                    TOPIC_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_SUBSCRIPTIONS", x => x.SUBSCRIPTION_ID);
                    table.ForeignKey(
                        name: "FK_FORUM_SUBSCRIPTIONS_FORUM_CATEGORY_CAT_ID",
                        column: x => x.CAT_ID,
                        principalTable: $"{_forumTablePrefix}CATEGORY",
                        principalColumn: "CAT_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FORUM_SUBSCRIPTIONS_FORUM_FORUM_FORUM_ID",
                        column: x => x.FORUM_ID,
                        principalTable: $"{_forumTablePrefix}FORUM",
                        principalColumn: "FORUM_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FORUM_SUBSCRIPTIONS_FORUM_MEMBERS_MEMBER_ID",
                        column: x => x.MEMBER_ID,
                        principalTable: $"{_memberTablePrefix}MEMBERS",
                        principalColumn: "MEMBER_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FORUM_SUBSCRIPTIONS_FORUM_TOPICS_TOPIC_ID",
                        column: x => x.TOPIC_ID,
                        principalTable: $"{_forumTablePrefix}TOPICS",
                        principalColumn: "TOPIC_ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}TOPICS",
                columns: table => new
                {
                    TOPIC_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CAT_ID = table.Column<int>(type: "int", nullable: false),
                    FORUM_ID = table.Column<int>(type: "int", nullable: false),
                    T_STATUS = table.Column<short>(type: "smallint", nullable: false),
                    T_MAIL = table.Column<short>(type: "smallint", nullable: false),
                    T_SUBJECT = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    T_MESSAGE = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    T_AUTHOR = table.Column<int>(type: "int", nullable: false),
                    T_REPLIES = table.Column<int>(type: "int", nullable: false),
                    T_UREPLIES = table.Column<int>(type: "int", nullable: false),
                    T_VIEW_COUNT = table.Column<int>(type: "int", nullable: false),
                    T_LAST_POST = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    T_DATE = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    T_IP = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    T_LAST_POST_AUTHOR = table.Column<int>(type: "int", nullable: true),
                    T_LAST_POST_REPLY_ID = table.Column<int>(type: "int", nullable: true),
                    T_ARCHIVE_FLAG = table.Column<int>(type: "int", nullable: true),
                    T_LAST_EDIT = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    T_LAST_EDITBY = table.Column<int>(type: "int", nullable: true),
                    T_STICKY = table.Column<short>(type: "smallint", nullable: false),
                    T_SIG = table.Column<short>(type: "smallint", nullable: false),
                    T_ISPOLL = table.Column<short>(type: "smallint", nullable: true),
                    T_POLLSTATUS = table.Column<short>(type: "smallint", nullable: true),
                    T_RATING_TOTAL_COUNT = table.Column<int>(type: "int", nullable: true),
                    T_RATING_TOTAL = table.Column<int>(type: "int", nullable: true),
                    T_ALLOW_RATING = table.Column<int>(type: "int", nullable: true),
                    T_ANSWERED = table.Column<bool>(type: "tinyint(1)", nullable: true,defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_TOPICS", x => x.TOPIC_ID);
                    table.ForeignKey(
                        name: "FK_FORUM_TOPICS_FORUM_CATEGORY_CAT_ID",
                        column: x => x.CAT_ID,
                        principalTable: $"{_forumTablePrefix}CATEGORY",
                        principalColumn: "CAT_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FORUM_TOPICS_FORUM_FORUM_FORUM_ID",
                        column: x => x.FORUM_ID,
                        principalTable: $"{_forumTablePrefix}FORUM",
                        principalColumn: "FORUM_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FORUM_TOPICS_FORUM_MEMBERS_T_AUTHOR",
                        column: x => x.T_AUTHOR,
                        principalTable: $"{_memberTablePrefix}MEMBERS",
                        principalColumn: "MEMBER_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FORUM_TOPICS_FORUM_MEMBERS_T_LAST_POST_AUTHOR",
                        column: x => x.T_LAST_POST_AUTHOR,
                        principalTable: $"{_memberTablePrefix}MEMBERS",
                        principalColumn: "MEMBER_ID");
                })
                .Annotation("MySql:CharSet", "utf8mb4");
            }
            else
            {
                migrationBuilder.AlterColumn<int>(
                    name: "F_A_COUNT",
                    table: $"{_forumTablePrefix}FORUM",
                    type: "int",
                    nullable: true,
                    oldClrType: typeof(double),
                    oldType: "double");
                migrationBuilder.AlterColumn<int>(
                    name: "F_A_TOPICS",
                    table: $"{_forumTablePrefix}FORUM",
                    type: "int",
                    nullable: true,
                    oldClrType: typeof(double),
                    oldType: "double");
                migrationBuilder.AlterColumn<int>(
                    name: "F_COUNT",
                    table: $"{_forumTablePrefix}FORUM",
                    type: "int",
                    nullable: true,
                    oldClrType: typeof(double),
                    oldType: "double");
                migrationBuilder.AlterColumn<int>(
                    name: "F_TOPICS",
                    table: $"{_forumTablePrefix}FORUM",
                    type: "int",
                    nullable: true,
                    oldClrType: typeof(double),
                    oldType: "double");
                migrationBuilder.AlterColumn<int>(
                    name: "T_REPLIES",
                    table: $"{_forumTablePrefix}TOPICS",
                    type: "int",
                    nullable: true,
                    oldClrType: typeof(double),
                    oldType: "double");

                if (!migrationBuilder.ColumnExists($"{_memberTablePrefix}MEMBERS", "M_PMEMAIL"))
                {
                    migrationBuilder.AddColumn<string>(
                        name: "M_PMEMAIL",
                        table: $"{_memberTablePrefix}MEMBERS",
                        type: "int",
                        nullable: true);
                }
                if (!migrationBuilder.ColumnExists($"{_memberTablePrefix}MEMBERS", "M_PMRECEIVE"))
                {
                    migrationBuilder.AddColumn<string>(
                        name: "M_PMRECEIVE",
                        table: $"{_memberTablePrefix}MEMBERS",
                        type: "int",
                        nullable: true);
                }
                if (!migrationBuilder.ColumnExists($"{_memberTablePrefix}MEMBERS", "M_PMSAVESENT"))
                {
                    migrationBuilder.AddColumn<string>(
                        name: "M_PMSAVESENT",
                        table: $"{_memberTablePrefix}MEMBERS",
                        type: "smallint",
                        nullable: true);
                }
                if (!migrationBuilder.ColumnExists($"{_memberTablePrefix}MEMBERS", "M_PRIVATEPROFILE"))
                {
                    migrationBuilder.AddColumn<string>(
                        name: "M_PRIVATEPROFILE",
                        table: $"{_memberTablePrefix}MEMBERS",
                        type: "smallint",
                        nullable: true);
                }
                if (!migrationBuilder.ColumnExists($"{_memberTablePrefix}MEMBERS", "M_LASTACTIVITY"))
                {
                    migrationBuilder.AddColumn<string>(
                        name: "M_LASTACTIVITY",
                        table: $"{_memberTablePrefix}MEMBERS",
                        type: "varchar(20)",
                        maxLength: 20,
                        nullable: true);
                }

                if (!migrationBuilder.ColumnExists($"{_forumTablePrefix}FORUM", "F_POLLS"))
                {
                    migrationBuilder.AddColumn<string>(
                        name: "F_POLLS",
                        table: $"{_forumTablePrefix}FORUM",
                        type: "int",
                        nullable: true);
                }
                if (!migrationBuilder.ColumnExists($"{_forumTablePrefix}FORUM", "F_RATING"))
                {
                    migrationBuilder.AddColumn<string>(
                        name: "F_RATING",
                        table: $"{_forumTablePrefix}FORUM",
                        type: "smallint",
                        nullable: true);
                }
                if (!migrationBuilder.ColumnExists($"{_forumTablePrefix}FORUM", "F_POSTAUTH"))
                {
                    migrationBuilder.AddColumn<string>(
                        name: "F_POSTAUTH",
                        table: $"{_forumTablePrefix}FORUM",
                        type: "int",
                        nullable: true);
                }
                if (!migrationBuilder.ColumnExists($"{_forumTablePrefix}FORUM", "F_REPLYAUTH"))
                {
                    migrationBuilder.AddColumn<string>(
                        name: "F_REPLYAUTH",
                        table: $"{_forumTablePrefix}FORUM",
                        type: "int",
                        nullable: true);
                }

                if (!migrationBuilder.ColumnExists($"{_forumTablePrefix}REPLY", "R_RATING"))
                {
                    migrationBuilder.AddColumn<string>(
                        name: "R_RATING",
                        table: $"{_forumTablePrefix}REPLY",
                        type: "int",
                        nullable: true);
                }
                if (!migrationBuilder.ColumnExists($"{_forumTablePrefix}REPLY", "R_ANSWER"))
                {
                    migrationBuilder.AddColumn<string>(
                        name: "R_ANSWER",
                        table: $"{_forumTablePrefix}REPLY",
                        type: "tinyint(1)",
                        nullable: true);
                }

                if (!migrationBuilder.ColumnExists($"{_forumTablePrefix}TOPICS", "T_ISPOLL"))
                {
                    migrationBuilder.AddColumn<string>(
                        name: "T_ISPOLL",
                        table: $"{_forumTablePrefix}TOPICS",
                        type: "smallint",
                        nullable: true);
                }
                if (!migrationBuilder.ColumnExists($"{_forumTablePrefix}TOPICS", "T_POLLSTATUS"))
                {
                    migrationBuilder.AddColumn<string>(
                        name: "T_POLLSTATUS",
                        table: $"{_forumTablePrefix}TOPICS",
                        type: "smallint",
                        nullable: true);
                }
                if (!migrationBuilder.ColumnExists($"{_forumTablePrefix}TOPICS", "T_RATING_TOTAL_COUNT"))
                {
                    migrationBuilder.AddColumn<string>(
                        name: "T_RATING_TOTAL_COUNT",
                        table: $"{_forumTablePrefix}TOPICS",
                        type: "int",
                        nullable: true);
                }
                if (!migrationBuilder.ColumnExists($"{_forumTablePrefix}TOPICS", "T_RATING_TOTAL"))
                {
                    migrationBuilder.AddColumn<string>(
                        name: "T_RATING_TOTAL",
                        table: $"{_forumTablePrefix}TOPICS",
                        type: "int",
                        nullable: true);
                }
                if (!migrationBuilder.ColumnExists($"{_forumTablePrefix}TOPICS", "T_ALLOW_RATING"))
                {
                    migrationBuilder.AddColumn<string>(
                        name: "T_ALLOW_RATING",
                        table: $"{_forumTablePrefix}TOPICS",
                        type: "int",
                        nullable: true);
                }
                if (!migrationBuilder.ColumnExists($"{_forumTablePrefix}TOPICS", "T_ANSWERED"))
                {
                    migrationBuilder.AddColumn<string>(
                        name: "T_ANSWERED",
                        table: $"{_forumTablePrefix}TOPICS",
                        type: "tinyint(1)",
                        nullable: false,
                        defaultValue: 1);
                }

                if (!migrationBuilder.ColumnExists($"{_forumTablePrefix}A_REPLY", "R_RATING"))
                {
                    migrationBuilder.AddColumn<string>(
                        name: "R_RATING",
                        table: $"{_forumTablePrefix}A_REPLY",
                        type: "int",
                        nullable: true);
                }
                if (!migrationBuilder.ColumnExists($"{_forumTablePrefix}A_REPLY", "R_ANSWER"))
                {
                    migrationBuilder.AddColumn<string>(
                        name: "R_ANSWER",
                        table: $"{_forumTablePrefix}A_REPLY",
                        type: "tinyint(1)",
                        nullable: true);
                }

                if (!migrationBuilder.ColumnExists($"{_forumTablePrefix}A_TOPICS", "T_ISPOLL"))
                {
                    migrationBuilder.AddColumn<string>(
                        name: "T_ISPOLL",
                        table: $"{_forumTablePrefix}A_TOPICS",
                        type: "smallint",
                        nullable: true);
                }
                if (!migrationBuilder.ColumnExists($"{_forumTablePrefix}A_TOPICS", "T_POLLSTATUS"))
                {
                    migrationBuilder.AddColumn<string>(
                        name: "T_POLLSTATUS",
                        table: $"{_forumTablePrefix}A_TOPICS",
                        type: "smallint",
                        nullable: true);
                }
                if (!migrationBuilder.ColumnExists($"{_forumTablePrefix}A_TOPICS", "T_RATING_TOTAL_COUNT"))
                {
                    migrationBuilder.AddColumn<string>(
                        name: "T_RATING_TOTAL_COUNT",
                        table: $"{_forumTablePrefix}A_TOPICS",
                        type: "int",
                        nullable: true);
                }
                if (!migrationBuilder.ColumnExists($"{_forumTablePrefix}A_TOPICS", "T_RATING_TOTAL"))
                {
                    migrationBuilder.AddColumn<string>(
                        name: "T_RATING_TOTAL",
                        table: $"{_forumTablePrefix}A_TOPICS",
                        type: "int",
                        nullable: true);
                }
                if (!migrationBuilder.ColumnExists($"{_forumTablePrefix}A_TOPICS", "T_ALLOW_RATING"))
                {
                    migrationBuilder.AddColumn<string>(
                        name: "T_ALLOW_RATING",
                        table: $"{_forumTablePrefix}A_TOPICS",
                        type: "int",
                        nullable: true);
                }
                if (!migrationBuilder.ColumnExists($"{_forumTablePrefix}A_TOPICS", "T_ANSWERED"))
                {
                    migrationBuilder.AddColumn<string>(
                        name: "T_ANSWERED",
                        table: $"{_forumTablePrefix}A_TOPICS",
                        type: "tinyint(1)",
                        nullable: true);
                }
            }
            if(!migrationBuilder.TableExists($"{_forumTablePrefix}CONFIG_NEW"))
            {  
                migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}CONFIG_NEW",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    C_VARIABLE = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    C_VALUE = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_CONFIG_NEW", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");                
            }
            if (!migrationBuilder.TableExists($"AspNetUsers"))
            {
                migrationBuilder.CreateTable(
                    name: "AspNetUsers",
                    columns: table => new
                    {
                        Id = table.Column<string>(type: "varchar(255)", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        Discriminator = table.Column<string>(type: "varchar(13)", maxLength: 13, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        MemberId = table.Column<int>(type: "int", nullable: true),
                        UserDescription = table.Column<string>(type: "longtext", nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        ProfileImageUrl = table.Column<string>(type: "longtext", nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        Rating = table.Column<int>(type: "int", nullable: true),
                        IsAdmin = table.Column<bool>(type: "tinyint(1)", nullable: true),
                        IsActive = table.Column<bool>(type: "tinyint(1)", nullable: true),
                        MemberSince = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                        UserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        NormalizedUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        NormalizedEmail = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        EmailConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                        PasswordHash = table.Column<string>(type: "longtext", nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        SecurityStamp = table.Column<string>(type: "longtext", nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        PhoneNumber = table.Column<string>(type: "longtext", nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        PhoneNumberConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                        TwoFactorEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                        LockoutEnd = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                        LockoutEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                        AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                        table.ForeignKey(
                            name: $"FK_AspNetUsers_FORUM_MEMBERS_MemberId",
                            column: x => x.MemberId,
                            principalTable: $"{_memberTablePrefix}MEMBERS",
                            principalColumn: "MEMBER_ID",
                            onDelete: ReferentialAction.Cascade);
                    })
                    .Annotation("MySql:CharSet", "utf8mb4");
                migrationBuilder.CreateTable(
                    name: "AspNetUserClaims",
                    columns: table => new
                    {
                        Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                        UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        ClaimType = table.Column<string>(type: "longtext", nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4")
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                        table.ForeignKey(
                            name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                            column: x => x.UserId,
                            principalTable: "AspNetUsers",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                    })
                    .Annotation("MySql:CharSet", "utf8mb4");
                migrationBuilder.CreateTable(
                    name: "AspNetUserLogins",
                    columns: table => new
                    {
                        LoginProvider = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        ProviderKey = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        ProviderDisplayName = table.Column<string>(type: "longtext", nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4")
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                        table.ForeignKey(
                            name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                            column: x => x.UserId,
                            principalTable: "AspNetUsers",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                    })
                    .Annotation("MySql:CharSet", "utf8mb4");
                migrationBuilder.CreateTable(
                    name: "AspNetUserRoles",
                    columns: table => new
                    {
                        UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        RoleId = table.Column<string>(type: "varchar(255)", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4")
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                        table.ForeignKey(
                            name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                            column: x => x.RoleId,
                            principalTable: "AspNetRoles",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                        table.ForeignKey(
                            name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                            column: x => x.UserId,
                            principalTable: "AspNetUsers",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                    })
                    .Annotation("MySql:CharSet", "utf8mb4");
                migrationBuilder.CreateTable(
                    name: "AspNetUserTokens",
                    columns: table => new
                    {
                        UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        LoginProvider = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        Name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        Value = table.Column<string>(type: "longtext", nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4")
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                        table.ForeignKey(
                            name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                            column: x => x.UserId,
                            principalTable: "AspNetUsers",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                    })
                    .Annotation("MySql:CharSet", "utf8mb4");

            }

            if (!migrationBuilder.TableExists($"{_forumTablePrefix}PM"))
            {
                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}PM",
                    columns: table => new
                    {
                        M_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                        M_SUBJECT = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_FROM = table.Column<int>(type: "int", nullable: false),
                        M_TO = table.Column<int>(type: "int", nullable: false),
                        M_SENT = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_MESSAGE = table.Column<string>(type: "longtext", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_PMCOUNT = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_READ = table.Column<int>(type: "int", nullable: false),
                        M_MAIL = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        M_OUTBOX = table.Column<short>(type: "smallint", nullable: false),
                        PM_DEL_FROM = table.Column<int>(type: "int", nullable: false),
                        PM_DEL_TO = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_PM", x => x.M_ID);
                        table.ForeignKey(
                            name: "FK_FORUM_PM_FORUM_MEMBERS_M_FROM",
                            column: x => x.M_FROM,
                            principalTable: $"{_memberTablePrefix}MEMBERS",
                            principalColumn: "MEMBER_ID",
                            onDelete: ReferentialAction.Cascade);
                    })
                    .Annotation("MySql:CharSet", "utf8mb4");
                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}PM_BLOCKLIST",
                    columns: table => new
                    {
                        BL_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                        BL_MEMBER_ID = table.Column<int>(type: "int", nullable: false),
                        BL_BLOCKED_ID = table.Column<int>(type: "int", nullable: false),
                        BL_BLOCKED_NAME = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4")
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_PM_BLOCKLIST", x => x.BL_ID);
                    })
                    .Annotation("MySql:CharSet", "utf8mb4");

            }
            if (!migrationBuilder.TableExists($"{_forumTablePrefix}ORG_GROUP"))
            {
                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}ORG_GROUP",
                    columns: table => new
                    {
                        O_GROUP_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                        O_GROUP_ORDER = table.Column<int>(type: "int", nullable: false),
                        O_GROUP_NAME = table.Column<string>(type: "longtext", nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4")
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_ORG_GROUP", x => x.O_GROUP_ID);
                    })
                    .Annotation("MySql:CharSet", "utf8mb4");

            }
            if (!migrationBuilder.TableExists($"{_forumTablePrefix}RANKING"))
            {
                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}RANKING",
                    columns: table => new
                    {
                        RANK_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                        R_TITLE = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        R_IMAGE = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        R_POSTS = table.Column<int>(type: "int", nullable: true),
                        R_IMG_REPEAT = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_RANKING", x => x.RANK_ID);
                    })
                    .Annotation("MySql:CharSet", "utf8mb4");

            }
            if (!migrationBuilder.TableExists($"LANGUAGE_RES"))
            {
                migrationBuilder.CreateTable(
                    name: "LANGUAGE_RES",
                    columns: table => new
                    {
                        pk = table.Column<int>(type: "int", nullable: false)
                            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                        ResourceId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        Value = table.Column<string>(type: "longtext", nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        Culture = table.Column<string>(type: "varchar(6)", maxLength: 6, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        Type = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        ResourceSet = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4")
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_LANGUAGE_RES", x => x.pk);
                    })
                    .Annotation("MySql:CharSet", "utf8mb4");

            }
            if (!migrationBuilder.TableExists($"TOPIC_RATINGS"))
            {
                migrationBuilder.CreateTable(
                    name: "TOPIC_RATINGS",
                    columns: table => new
                    {
                        RATING = table.Column<int>(type: "int", nullable: false)
                            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                        RATINGS_BYMEMBER_ID = table.Column<int>(type: "int", nullable: false),
                        RATINGS_TOPIC_ID = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_TOPIC_RATINGS", x => x.RATING);
                    })
                    .Annotation("MySql:CharSet", "utf8mb4");

            }
            if (!migrationBuilder.TableExists($"{_forumTablePrefix}BOOKMARKS"))
            {
                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}BOOKMARKS",
                    columns: table => new
                    {
                        BOOKMARK_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                        B_TOPICID = table.Column<int>(type: "int", nullable: false),
                        B_MEMBERID = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_BOOKMARKS", x => x.BOOKMARK_ID);
                        table.ForeignKey(
                            name: "FK_FORUM_BOOKMARKS_FORUM_MEMBERS_B_MEMBERID",
                            column: x => x.B_MEMBERID,
                            principalTable: $"{_memberTablePrefix}MEMBERS",
                            principalColumn: "MEMBER_ID",
                            onDelete: ReferentialAction.Cascade);
                    })
                    .Annotation("MySql:CharSet", "utf8mb4");
            }

            if (!migrationBuilder.TableExists($"{_forumTablePrefix}POLLS"))
            {
                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}POLLS",
                    columns: table => new
                    {
                        POLL_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                        CAT_ID = table.Column<int>(type: "int", nullable: false),
                        FORUM_ID = table.Column<int>(type: "int", nullable: false),
                        TOPIC_ID = table.Column<int>(type: "int", nullable: false),
                        P_WHOVOTES = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        P_LASTVOTE = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        P_QUESTION = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4")
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_POLLS", x => x.POLL_ID);
                        table.ForeignKey(
                            name: "FK_FORUM_POLLS_FORUM_TOPICS_TOPIC_ID",
                            column: x => x.TOPIC_ID,
                            principalTable: $"{_forumTablePrefix}TOPICS",
                            principalColumn: "TOPIC_ID",
                            onDelete: ReferentialAction.Cascade);
                    })
                    .Annotation("MySql:CharSet", "utf8mb4");
                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}POLL_ANSWERS",
                    columns: table => new
                    {
                        POLLANSWER_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                        POLL_ID = table.Column<int>(type: "int", nullable: false),
                        POLLANSWER_LABEL = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        POLLANSWER_ORDER = table.Column<int>(type: "int", nullable: false),
                        POLLANSWER_COUNT = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_POLL_ANSWERS", x => x.POLLANSWER_ID);
                        table.ForeignKey(
                            name: "FK_FORUM_POLL_ANSWERS_FORUM_POLLS_POLL_ID",
                            column: x => x.POLL_ID,
                            principalTable: $"{_forumTablePrefix}POLLS",
                            principalColumn: "POLL_ID",
                            onDelete: ReferentialAction.Cascade);
                    })
                    .Annotation("MySql:CharSet", "utf8mb4");
                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}POLL_VOTES",
                    columns: table => new
                    {
                        POLLVOTES_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                        POLL_ID = table.Column<int>(type: "int", nullable: false),
                        CAT_ID = table.Column<int>(type: "int", nullable: false),
                        FORUM_ID = table.Column<int>(type: "int", nullable: false),
                        TOPIC_ID = table.Column<int>(type: "int", nullable: false),
                        MEMBER_ID = table.Column<int>(type: "int", nullable: false),
                        GUEST_VOTE = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_POLL_VOTES", x => x.POLLVOTES_ID);
                        table.ForeignKey(
                            name: "FK_FORUM_POLL_VOTES_FORUM_POLLS_POLL_ID",
                            column: x => x.POLL_ID,
                            principalTable: $"{_forumTablePrefix}POLLS",
                            principalColumn: "POLL_ID",
                            onDelete: ReferentialAction.Cascade);
                    })
                    .Annotation("MySql:CharSet", "utf8mb4");

            }

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_MemberId",
                table: "AspNetUsers",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_A_REPLY_R_AUTHOR",
                table: $"{_forumTablePrefix}A_REPLY",
                column: "R_AUTHOR");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_A_REPLY_TOPIC_ID",
                table: $"{_forumTablePrefix}A_REPLY",
                column: "TOPIC_ID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_FORUM_A_TOPICS_CAT_ID",
            //    table: $"{_forumTablePrefix}A_TOPICS",
            //    column: "CAT_ID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_FORUM_A_TOPICS_FORUM_ID",
            //    table: $"{_forumTablePrefix}A_TOPICS",
            //    column: "FORUM_ID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_FORUM_A_TOPICS_T_AUTHOR",
            //    table: $"{_forumTablePrefix}A_TOPICS",
            //    column: "T_AUTHOR");

            //migrationBuilder.CreateIndex(
            //    name: "IX_FORUM_A_TOPICS_T_LAST_POST_AUTHOR",
            //    table: $"{_forumTablePrefix}A_TOPICS",
            //    column: "T_LAST_POST_AUTHOR");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_ALLOWED_MEMBERS_FORUM_ID",
                table: $"{_forumTablePrefix}ALLOWED_MEMBERS",
                column: "FORUM_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_BOOKMARKS_B_MEMBERID",
                table: $"{_forumTablePrefix}BOOKMARKS",
                column: "B_MEMBERID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_BOOKMARKS_B_TOPICID",
                table: $"{_forumTablePrefix}BOOKMARKS",
                column: "B_TOPICID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_CONFIG_NEW_C_VARIABLE",
                table: $"{_forumTablePrefix}CONFIG_NEW",
                column: "C_VARIABLE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_FORUM_CAT_ID",
                table: $"{_forumTablePrefix}FORUM",
                column: "CAT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_FORUM_F_LAST_POST_AUTHOR",
                table: $"{_forumTablePrefix}FORUM",
                column: "F_LAST_POST_AUTHOR");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_FORUM_F_LAST_POST_TOPIC_ID",
                table: $"{_forumTablePrefix}FORUM",
                column: "F_LAST_POST_TOPIC_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_GROUPS_GROUP_CATID",
                table: $"{_forumTablePrefix}GROUPS",
                column: "GROUP_CATID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_GROUPS_GROUP_ID",
                table: $"{_forumTablePrefix}GROUPS",
                column: "GROUP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_MODERATOR_FORUM_ID",
                table: $"{_forumTablePrefix}MODERATOR",
                column: "FORUM_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_MODERATOR_MEMBER_ID",
                table: $"{_forumTablePrefix}MODERATOR",
                column: "MEMBER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_PM_M_FROM",
                table: $"{_forumTablePrefix}PM",
                column: "M_FROM");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_POLL_ANSWERS_POLL_ID",
                table: $"{_forumTablePrefix}POLL_ANSWERS",
                column: "POLL_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_POLL_VOTES_POLL_ID",
                table: $"{_forumTablePrefix}POLL_VOTES",
                column: "POLL_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_POLLS_TOPIC_ID",
                table: $"{_forumTablePrefix}POLLS",
                column: "TOPIC_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_REPLY_R_AUTHOR",
                table: $"{_forumTablePrefix}REPLY",
                column: "R_AUTHOR");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_REPLY_TOPIC_ID",
                table: $"{_forumTablePrefix}REPLY",
                column: "TOPIC_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_SUBSCRIPTIONS_CAT_ID",
                table: $"{_forumTablePrefix}SUBSCRIPTIONS",
                column: "CAT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_SUBSCRIPTIONS_FORUM_ID",
                table: $"{_forumTablePrefix}SUBSCRIPTIONS",
                column: "FORUM_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_SUBSCRIPTIONS_MEMBER_ID",
                table: $"{_forumTablePrefix}SUBSCRIPTIONS",
                column: "MEMBER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_SUBSCRIPTIONS_TOPIC_ID",
                table: $"{_forumTablePrefix}SUBSCRIPTIONS",
                column: "TOPIC_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_TOPICS_CAT_ID",
                table: $"{_forumTablePrefix}TOPICS",
                column: "CAT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_TOPICS_FORUM_ID",
                table: $"{_forumTablePrefix}TOPICS",
                column: "FORUM_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_TOPICS_T_AUTHOR",
                table: $"{_forumTablePrefix}TOPICS",
                column: "T_AUTHOR");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_TOPICS_T_LAST_POST_AUTHOR",
                table: $"{_forumTablePrefix}TOPICS",
                column: "T_LAST_POST_AUTHOR");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_FORUM_A_REPLY_FORUM_A_TOPICS_TOPIC_ID",
            //    table: $"{_forumTablePrefix}A_REPLY",
            //    column: "TOPIC_ID",
            //    principalTable: $"{_forumTablePrefix}A_TOPICS",
            //    principalColumn: "TOPIC_ID",
            //    onDelete: ReferentialAction.NoAction);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_FORUM_A_TOPICS_FORUM_FORUM_FORUM_ID",
            //    table: $"{_forumTablePrefix}A_TOPICS",
            //    column: "FORUM_ID",
            //    principalTable: $"{_forumTablePrefix}FORUM",
            //    principalColumn: "FORUM_ID",
            //    onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_FORUM_ALLOWED_MEMBERS_FORUM_FORUM_FORUM_ID",
                table: $"{_forumTablePrefix}ALLOWED_MEMBERS",
                column: "FORUM_ID",
                principalTable: $"{_forumTablePrefix}FORUM",
                principalColumn: "FORUM_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FORUM_BOOKMARKS_FORUM_TOPICS_B_TOPICID",
                table: $"{_forumTablePrefix}BOOKMARKS",
                column: "B_TOPICID",
                principalTable: $"{_forumTablePrefix}TOPICS",
                principalColumn: "TOPIC_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FORUM_FORUM_FORUM_TOPICS_F_LAST_POST_TOPIC_ID",
                table: $"{_forumTablePrefix}FORUM",
                column: "F_LAST_POST_TOPIC_ID",
                principalTable: $"{_forumTablePrefix}TOPICS",
                principalColumn: "TOPIC_ID");

            migrationBuilder.Sql(@"
DELIMITER $$
CREATE PROCEDURE `snitz_updatecounts`()
BEGIN

CREATE TEMPORARY TABLE F_T_COUNT (
FORUM_ID int,
T_COUNT int
);


UPDATE FORUM_FORUM 
SET F_TOPICS = 0;


INSERT INTO F_T_COUNT
  
SELECT FORUM_ID, COUNT(FORUM_ID) FROM FORUM_TOPICS WHERE T_STATUS<=1 GROUP By FORUM_ID ;


UPDATE FORUM_FORUM F INNER JOIN F_T_COUNT T ON T.FORUM_ID=F.FORUM_ID
  SET F_TOPICS = T_COUNT;

/* Update Forum Archived Topics Count  */

DELETE FROM F_T_COUNT;

UPDATE FORUM_FORUM SET F_A_TOPICS = 0;

INSERT INTO F_T_COUNT
  SELECT FORUM_ID, COUNT(FORUM_ID) FROM FORUM_A_TOPICS WHERE T_STATUS<=1 GROUP By FORUM_ID ;

UPDATE FORUM_FORUM F INNER JOIN F_T_COUNT T ON T.FORUM_ID=F.FORUM_ID
  SET F_A_TOPICS = T_COUNT;

DROP TABLE F_T_COUNT;

/* Update Topic Replies Counts now */

CREATE TEMPORARY TABLE T_R_COUNT (
   TOPIC_ID int(11),
   R_COUNT int(11)
);

INSERT INTO T_R_COUNT 
  SELECT TOPIC_ID, COUNT(REPLY_ID) FROM FORUM_REPLY WHERE R_STATUS<=1 GROUP BY TOPIC_ID ;

UPDATE FORUM_TOPICS SET T_UREPLIES = 0 WHERE T_STATUS<=1;

UPDATE FORUM_TOPICS T INNER JOIN T_R_COUNT TR ON T.TOPIC_ID = TR.TOPIC_ID
   SET T_REPLIES = R_COUNT WHERE T.T_STATUS<=1;

/* Update Archived Topics Replies Count Now */

DELETE FROM T_R_COUNT;

INSERT INTO T_R_COUNT 
  SELECT TOPIC_ID, COUNT(REPLY_ID) FROM FORUM_A_REPLY GROUP BY TOPIC_ID;

UPDATE FORUM_A_TOPICS SET T_UREPLIES = 0;

UPDATE FORUM_A_TOPICS T INNER JOIN T_R_COUNT TR ON T.TOPIC_ID = TR.TOPIC_ID
   SET T_REPLIES = R_COUNT;

DROP TABLE T_R_COUNT;

/* Update Last post Date */ 

CREATE TEMPORARY TABLE T_POST_DATA (
   TOPIC_ID int(11),
   LAST_POST varchar(50)
);

INSERT INTO T_POST_DATA
  SELECT TOPIC_ID, MAX(R_DATE) FROM FORUM_REPLY WHERE R_STATUS<=1 GROUP BY TOPIC_ID;

UPDATE FORUM_TOPICS T INNER JOIN T_POST_DATA TP  ON T.TOPIC_ID = TP.TOPIC_ID
  SET T.T_LAST_POST = LAST_POST;

DELETE FROM T_POST_DATA;

UPDATE FORUM_TOPICS SET T_LAST_POST=T_DATE, T_LAST_POST_AUTHOR=T_AUTHOR, T_LAST_POST_REPLY_ID=0 WHERE T_REPLIES=0;

/* Update Last post Date */ 

INSERT INTO T_POST_DATA
  SELECT TOPIC_ID, MAX(R_DATE) FROM FORUM_A_REPLY GROUP BY TOPIC_ID;

UPDATE FORUM_A_TOPICS T INNER JOIN T_POST_DATA TP  ON T.TOPIC_ID = TP.TOPIC_ID
  SET T.T_LAST_POST = TP.LAST_POST;

UPDATE FORUM_A_TOPICS SET T_LAST_POST=T_DATE, T_LAST_POST_AUTHOR=T_AUTHOR,T_LAST_POST_REPLY_ID=0 WHERE T_REPLIES=0;

DROP TABLE T_POST_DATA;

/* Now find the reply ID for the posts that have more than 0 replies */

CREATE TEMPORARY TABLE T_L_REPLY_ID (
   TOPIC_ID int(11),
   REPLY_ID int(11)
);

INSERT INTO T_L_REPLY_ID
  SELECT T.TOPIC_ID, MAX(REPLY_ID) FROM FORUM_REPLY R INNER JOIN FORUM_TOPICS T ON R.TOPIC_ID=T.TOPIC_ID
       WHERE T.T_LAST_POST=R_DATE AND T_STATUS<=1 GROUP BY T.TOPIC_ID;

UPDATE FORUM_TOPICS T INNER JOIN T_L_REPLY_ID TL ON TL.TOPIC_ID = T.TOPIC_ID
  SET T.T_LAST_POST_REPLY_ID = TL.REPLY_ID;

DELETE FROM T_L_REPLY_ID;

/* Now find the reply ID for the posts that have more than 0 replies in archived topics */

INSERT INTO T_L_REPLY_ID
  SELECT T.TOPIC_ID, MAX(REPLY_ID) FROM FORUM_A_REPLY R INNER JOIN FORUM_A_TOPICS T ON R.TOPIC_ID=T.TOPIC_ID
       WHERE T.T_LAST_POST=R_DATE GROUP BY T.TOPIC_ID;

UPDATE FORUM_A_TOPICS T INNER JOIN T_L_REPLY_ID TL ON TL.TOPIC_ID = T.TOPIC_ID
  SET T.T_LAST_POST_REPLY_ID = TL.REPLY_ID;

DROP TABLE T_L_REPLY_ID;

/* Now found the author ID for the last reply */

CREATE TEMPORARY TABLE T_L_REPLY_AUTHOR(
   TOPIC_ID int(11),
   AUTHOR int(11)
);

INSERT INTO T_L_REPLY_AUTHOR
  SELECT T.TOPIC_ID, R.R_AUTHOR FROM FORUM_TOPICS T INNER JOIN FORUM_REPLY R ON T.TOPIC_ID=R.TOPIC_ID
    WHERE T.T_LAST_POST_REPLY_ID = R.REPLY_ID AND T_STATUS<=1;

UPDATE FORUM_TOPICS T INNER JOIN T_L_REPLY_AUTHOR TL ON TL.TOPIC_ID = T.TOPIC_ID
  SET T.T_LAST_POST_AUTHOR = TL.AUTHOR;

DELETE FROM T_L_REPLY_AUTHOR;

INSERT INTO T_L_REPLY_AUTHOR
  SELECT T.TOPIC_ID, R.R_AUTHOR FROM FORUM_A_TOPICS T INNER JOIN FORUM_A_REPLY R ON T.TOPIC_ID=R.TOPIC_ID
    WHERE T.T_LAST_POST_REPLY_ID = R.REPLY_ID;

UPDATE FORUM_A_TOPICS T INNER JOIN T_L_REPLY_AUTHOR TL ON TL.TOPIC_ID = T.TOPIC_ID
  SET T.T_LAST_POST_AUTHOR = TL.AUTHOR;

DROP TABLE T_L_REPLY_AUTHOR;

/* Now to current step 3, unmoderated replies per topic */


/* Update Topic Replies Counts now */

CREATE TEMPORARY TABLE T_R_COUNT (
   TOPIC_ID int(11),
   R_COUNT int(11)
);

INSERT INTO T_R_COUNT 
  SELECT TOPIC_ID, COUNT(REPLY_ID) FROM FORUM_REPLY WHERE R_STATUS=2 OR R_STATUS=3 GROUP BY TOPIC_ID ;

UPDATE FORUM_TOPICS SET T_UREPLIES = 0 WHERE T_STATUS<=1;

UPDATE FORUM_TOPICS T INNER JOIN T_R_COUNT TR ON T.TOPIC_ID = TR.TOPIC_ID
   SET T_UREPLIES = R_COUNT WHERE T.T_STATUS<=1;

DROP TABLE T_R_COUNT;

/* Now to step 4 */

/* Count replies per forum */

CREATE TEMPORARY TABLE F_R_COUNT (
    FORUM_ID int(11),
    R_COUNT int (11)
);

INSERT INTO F_R_COUNT 
  SELECT R.FORUM_ID, COUNT(REPLY_ID) FROM FORUM_TOPICS T INNER JOIN FORUM_REPLY R On T.TOPIC_ID=R.TOPIC_ID
     WHERE T.T_STATUS<=1 AND R_STATUS<=1 GROUP By R.FORUM_ID;

UPDATE FORUM_FORUM SET F_COUNT=F_TOPICS WHERE F_TYPE<>1;

UPDATE FORUM_FORUM F INNER JOIN F_R_COUNT FR ON F.FORUM_ID = FR.FORUM_ID
   SET F.F_COUNT = F.F_COUNT + FR.R_COUNT;

DELETE FROM F_R_COUNT;

INSERT INTO F_R_COUNT 
  SELECT R.FORUM_ID, COUNT(REPLY_ID) FROM FORUM_A_TOPICS T INNER JOIN FORUM_A_REPLY R On T.TOPIC_ID=R.TOPIC_ID GROUP By R.FORUM_ID;

UPDATE FORUM_FORUM SET F_A_COUNT=F_A_TOPICS WHERE F_TYPE<>1;

UPDATE FORUM_FORUM F INNER JOIN F_R_COUNT FR ON F.FORUM_ID = FR.FORUM_ID
   SET F.F_A_COUNT = F.F_A_COUNT + FR.R_COUNT;

DROP TABLE F_R_COUNT;

/* Update Last Post Per Forum */

CREATE TEMPORARY TABLE F_POST_DATA (
   FORUM_ID int(11),
   LAST_POST varchar(50)
);


INSERT INTO F_POST_DATA
  SELECT FORUM_ID, MAX(T_LAST_POST) FROM FORUM_TOPICS WHERE T_STATUS<=1 GROUP BY FORUM_ID;

UPDATE FORUM_FORUM F INNER JOIN F_POST_DATA FP  ON F.FORUM_ID = FP.FORUM_ID
  SET F.F_LAST_POST = LAST_POST;

DROP TABLE F_POST_DATA;

/* Update Last Post TOPIC_ID */


CREATE TEMPORARY TABLE F_TOPIC_ID (
   FORUM_ID int(11),
   TOPIC_ID int(11)
);

INSERT INTO F_TOPIC_ID
  SELECT F.FORUM_ID, MAX(T.TOPIC_ID) FROM FORUM_FORUM F INNER JOIN FORUM_TOPICS T On F.FORUM_ID=T.FORUM_ID
   WHERE F.F_LAST_POST = T.T_LAST_POST and T.T_STATUS<=1 GROUP BY F.FORUM_ID;

UPDATE FORUM_FORUM F INNER JOIN F_TOPIC_ID FT ON F.FORUM_ID = FT.FORUM_ID
   SET F.F_LAST_POST_TOPIC_ID = FT.TOPIC_ID;

DROP TABLE F_TOPIC_ID;

/* Now Update for Author ID */

UPDATE FORUM_FORUM F INNER JOIN FORUM_TOPICS T On F.F_LAST_POST_TOPIC_ID=T.TOPIC_ID
SET F.F_LAST_POST_AUTHOR=T.T_LAST_POST_AUTHOR, F.F_LAST_POST_REPLY_ID=T.T_LAST_POST_REPLY_ID;

CREATE TEMPORARY TABLE T_TOPICS (
   COUNT_ID int(11),
   TOPICS int(11),
   A_TOPICS int(11)
);

INSERT INTO T_TOPICS
  SELECT 1, SUM(F_TOPICS), SUM(F_A_TOPICS) FROM FORUM_FORUM;

UPDATE FORUM_TOTALS FT INNER JOIN T_TOPICS TT ON FT.COUNT_ID=TT.COUNT_ID
  SET FT.T_COUNT = TT.TOPICS, FT.T_A_COUNT = A_TOPICS;

DROP TABLE T_TOPICS;

CREATE TEMPORARY TABLE T_TOPICS (
   COUNT_ID int(11),
   TOPICS int(11),
   A_TOPICS int(11),
   POSTS int(11),
   A_POSTS int(11)
);

INSERT INTO T_TOPICS
  SELECT 1, SUM(F_TOPICS), SUM(F_A_TOPICS),SUM(F_COUNT),SUM(F_A_COUNT) FROM FORUM_FORUM WHERE F_TYPE<>1;

UPDATE FORUM_TOTALS FT INNER JOIN T_TOPICS TT ON FT.COUNT_ID=TT.COUNT_ID
  SET FT.T_COUNT = TT.TOPICS, FT.T_A_COUNT = A_TOPICS, FT.P_COUNT=TT.POSTS, FT.P_A_COUNT=TT.A_POSTS;

DROP TABLE T_TOPICS;

CREATE TEMPORARY TABLE T_MEMBERS (
   COUNT_ID int(11),
   MEMBERS int(11)
);
 
INSERT INTO T_MEMBERS
  SELECT 1, COUNT(MEMBER_ID) FROM FORUM_MEMBERS;

UPDATE FORUM_TOTALS FT INNER JOIN T_MEMBERS TM ON FT.COUNT_ID=TM.COUNT_ID
  SET FT.U_COUNT = TM.MEMBERS; 

DROP TABLE T_MEMBERS;

END$$
DELIMITER ;
            ");


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FORUM_FORUM_FORUM_MEMBERS_F_LAST_POST_AUTHOR",
                table: $"{_forumTablePrefix}FORUM");

            migrationBuilder.DropForeignKey(
                name: "FK_FORUM_TOPICS_FORUM_MEMBERS_T_AUTHOR",
                table: $"{_forumTablePrefix}TOPICS");

            migrationBuilder.DropForeignKey(
                name: "FK_FORUM_TOPICS_FORUM_MEMBERS_T_LAST_POST_AUTHOR",
                table: $"{_forumTablePrefix}TOPICS");

            migrationBuilder.DropForeignKey(
                name: "FK_FORUM_FORUM_FORUM_TOPICS_F_LAST_POST_TOPIC_ID",
                table: $"{_forumTablePrefix}FORUM");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}A_REPLY");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}ALLOWED_MEMBERS");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}BADWORDS");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}BOOKMARKS");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}CONFIG_NEW");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}GROUPS");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}IMAGES");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}MODERATOR");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}NAMEFILTER");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}PM");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}PM_BLOCKLIST");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}POLL_ANSWERS");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}POLL_VOTES");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}RANKING");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}REPLY");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}SPAM_MAIL");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}SUBSCRIPTIONS");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}TOTALS");

            migrationBuilder.DropTable(
                name: "LANGUAGE_RES");

            migrationBuilder.DropTable(
                name: "TOPIC_RATINGS");

            //migrationBuilder.DropTable(
            //    name: "webpages_Membership");

            //migrationBuilder.DropTable(
            //    name: "webpages_UsersInRoles");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}A_TOPICS");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}GROUP_NAMES");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}IMAGE_CAT");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}ORG_GROUP");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}POLLS");

            //migrationBuilder.DropTable(
            //    name: "webpages_Roles");

            migrationBuilder.DropTable(
                name: $"{_memberTablePrefix}MEMBERS");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}TOPICS");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}FORUM");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}CATEGORY");
        }
    }
}
