using Microsoft.EntityFrameworkCore.Migrations;
using SnitzCore.Data.Models;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class SnitzForums2000 : SnitzMigration
    {

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            SetParameters();
            Console.WriteLine("Creating Snitz base Tables");
            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}A_REPLY",
                columns: table => new
                {
                    REPLY_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CAT_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    FORUM_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    TOPIC_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    R_MAIL = table.Column<short>(type: "INTEGER", nullable: false),
                    R_AUTHOR = table.Column<int>(type: "INTEGER", nullable: false),
                    R_MESSAGE = table.Column<string>(type: "TEXT", nullable: false),
                    R_DATE = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                    R_IP = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    R_STATUS = table.Column<short>(type: "INTEGER", nullable: false),
                    R_LAST_EDIT = table.Column<string>(type: "TEXT", maxLength: 14, nullable: true),
                    R_LAST_EDITBY = table.Column<int>(type: "INTEGER", nullable: true),
                    R_SIG = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_A_REPLY", x => x.REPLY_ID);
                });

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}A_TOPICS",
                columns: table => new
                {
                    TOPIC_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CAT_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    FORUM_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    T_STATUS = table.Column<short>(type: "INTEGER", nullable: false),
                    T_MAIL = table.Column<short>(type: "INTEGER", nullable: false),
                    T_SUBJECT = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    T_MESSAGE = table.Column<string>(type: "TEXT", nullable: false),
                    T_AUTHOR = table.Column<int>(type: "INTEGER", nullable: false),
                    T_REPLIES = table.Column<int>(type: "INTEGER", nullable: false),
                    T_UREPLIES = table.Column<int>(type: "INTEGER", nullable: false),
                    T_VIEW_COUNT = table.Column<int>(type: "INTEGER", nullable: false),
                    T_LAST_POST = table.Column<string>(type: "TEXT", maxLength: 14, nullable: true),
                    T_DATE = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                    LastPoster = table.Column<int>(type: "INTEGER", nullable: true),
                    T_IP = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    T_LAST_POST_AUTHOR = table.Column<int>(type: "INTEGER", nullable: true),
                    T_LAST_POST_REPLY_ID = table.Column<int>(type: "INTEGER", nullable: true),
                    T_ARCHIVE_FLAG = table.Column<int>(type: "INTEGER", nullable: true),
                    T_LAST_EDIT = table.Column<string>(type: "TEXT", maxLength: 14, nullable: true),
                    T_LAST_EDITBY = table.Column<int>(type: "INTEGER", nullable: true),
                    T_STICKY = table.Column<short>(type: "INTEGER", nullable: false),
                    T_SIG = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_A_TOPICS", x => x.TOPIC_ID);
                });

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}BADWORDS",
                columns: table => new
                {
                    B_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    B_BADWORD = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    B_REPLACE = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_BADWORDS", x => x.B_ID);
                });

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}CATEGORY",
                columns: table => new
                {
                    CAT_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CAT_STATUS = table.Column<short>(type: "INTEGER", nullable: true),
                    CAT_NAME = table.Column<string>(type: "TEXT", nullable: true),
                    CAT_MODERATION = table.Column<int>(type: "INTEGER", nullable: true),
                    CAT_SUBSCRIPTION = table.Column<int>(type: "INTEGER", nullable: true),
                    CAT_ORDER = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_CATEGORY", x => x.CAT_ID);
                });

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}CONFIG_NEW",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    C_VARIABLE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    C_VALUE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_CONFIG_NEW", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}GROUP_NAMES",
                columns: table => new
                {
                    GROUP_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GROUP_NAME = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    GROUP_DESCRIPTION = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    GROUP_ICON = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    GROUP_IMAGE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_GROUP_NAMES", x => x.GROUP_ID);
                });

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}GROUPS",
                columns: table => new
                {
                    GROUP_KEY = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GROUP_ID = table.Column<int>(type: "INTEGER", nullable: true),
                    GROUP_CATID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_GROUPS", x => x.GROUP_KEY);
                });

            migrationBuilder.CreateTable(
                name: $"{_memberTablePrefix}MEMBERS",
                columns: table => new
                {
                    MEMBER_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    M_NAME = table.Column<string>(type: "TEXT", maxLength: 75, nullable: false),
                    M_EMAIL = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    M_TITLE = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    M_LEVEL = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: 1),
                    M_STATUS = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: 1),
                    M_POSTS = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    M_LASTPOSTDATE = table.Column<string>(type: "TEXT", maxLength: 14, nullable: true),
                    M_DATE = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                    M_FIRSTNAME = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    M_LASTNAME = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    M_CITY = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    M_STATE = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    M_PHOTO_URL = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    M_COUNTRY = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    M_SEX = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    M_AGE = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    M_DOB = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true),
                    M_OCCUPATION = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    M_HOMEPAGE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    M_MARSTATUS = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    M_DEFAULT_VIEW = table.Column<int>(type: "INTEGER", nullable: false,defaultValue:0),
                    M_SIG = table.Column<string>(type: "TEXT", nullable: true),
                    M_VIEW_SIG = table.Column<short>(type: "INTEGER", nullable: false,defaultValue:0),
                    M_SIG_DEFAULT = table.Column<short>(type: "INTEGER", nullable: false,defaultValue:0),
                    M_HIDE_EMAIL = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: 0),
                    M_RECEIVE_EMAIL = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: 1),
                    M_LAST_IP = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    M_IP = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    M_ALLOWEMAIL = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: 1),
                    M_SUBSCRIPTION = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: 1),
                    M_KEY = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    M_NEWEMAIL = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    M_PWKEY = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    M_SHA256 = table.Column<short>(type: "INTEGER", nullable: true, defaultValue: 1),
                    M_AIM = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    M_ICQ = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    M_MSN = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    M_YAHOO = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    M_HOBBIES = table.Column<string>(type: "TEXT", nullable: true),
                    M_LNEWS = table.Column<string>(type: "TEXT", nullable: true),
                    M_QUOTE = table.Column<string>(type: "TEXT", nullable: true),
                    M_BIO = table.Column<string>(type: "TEXT", nullable: true),
                    M_LINK1 = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    M_LINK2 = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_MEMBERS", x => x.MEMBER_ID);
                });

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}MODERATOR",
                columns: table => new
                {
                    MOD_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FORUM_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    MEMBER_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    MOD_TYPE = table.Column<short>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_MODERATOR", x => x.MOD_ID);
                });

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}NAMEFILTER",
                columns: table => new
                {
                    N_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    N_NAME = table.Column<string>(type: "TEXT", maxLength: 75, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_NAMEFILTER", x => x.N_ID);
                });

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}SUBSCRIPTIONS",
                columns: table => new
                {
                    SUBSCRIPTION_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MEMBER_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    CAT_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    FORUM_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    TOPIC_ID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_SUBSCRIPTIONS", x => x.SUBSCRIPTION_ID);
                });

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}TOTALS",
                columns: table => new
                {
                    COUNT_ID = table.Column<short>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    P_COUNT = table.Column<int>(type: "INTEGER", nullable: false),
                    P_A_COUNT = table.Column<int>(type: "INTEGER", nullable: false),
                    T_COUNT = table.Column<int>(type: "INTEGER", nullable: false),
                    T_A_COUNT = table.Column<int>(type: "INTEGER", nullable: false),
                    U_COUNT = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_TOTALS", x => x.COUNT_ID);
                });

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}FORUM",
                columns: table => new
                {
                    FORUM_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CAT_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    F_STATUS = table.Column<short>(type: "INTEGER", nullable: false),
                    F_MAIL = table.Column<short>(type: "INTEGER", nullable: false),
                    F_SUBJECT = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    F_URL = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    F_DESCRIPTION = table.Column<string>(type: "TEXT", nullable: false),
                    F_TOPICS = table.Column<int>(type: "INTEGER", nullable: false),
                    F_COUNT = table.Column<int>(type: "INTEGER", nullable: false),
                    F_LAST_POST = table.Column<string>(type: "TEXT", maxLength: 14, nullable: true),
                    F_PASSWORD_NEW = table.Column<string>(type: "TEXT", nullable: true),
                    F_PRIVATEFORUMS = table.Column<int>(type: "INTEGER", nullable: false),
                    F_TYPE = table.Column<short>(type: "INTEGER", nullable: false),
                    F_IP = table.Column<string>(type: "TEXT", nullable: true),
                    F_LAST_POST_AUTHOR = table.Column<int>(type: "INTEGER", nullable: true),
                    F_LAST_POST_TOPIC_ID = table.Column<int>(type: "INTEGER", nullable: true),
                    F_LAST_POST_REPLY_ID = table.Column<int>(type: "INTEGER", nullable: true),
                    F_A_TOPICS = table.Column<int>(type: "INTEGER", nullable: false,defaultValue:0),
                    F_A_COUNT = table.Column<int>(type: "INTEGER", nullable: false,defaultValue:0),
                    F_MODERATION = table.Column<int>(type: "INTEGER", nullable: false),
                    F_SUBSCRIPTION = table.Column<int>(type: "INTEGER", nullable: false),
                    F_ORDER = table.Column<int>(type: "INTEGER", nullable: false),
                    F_DEFAULTDAYS = table.Column<int>(type: "INTEGER", nullable: false),
                    F_COUNT_M_POSTS = table.Column<short>(type: "INTEGER", nullable: false),
                    F_L_ARCHIVE = table.Column<string>(type: "TEXT", nullable: true),
                    F_ARCHIVE_SCHED = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 60),
                    F_L_DELETE = table.Column<string>(type: "TEXT", nullable: true),
                    F_DELETE_SCHED = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 365),

                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_FORUM", x => x.FORUM_ID);
                    table.ForeignKey(
                        name: "FK_FORUM_FORUM_FORUM_CATEGORY_CAT_ID",
                        column: x => x.CAT_ID,
                        principalTable: $"{_forumTablePrefix}CATEGORY",
                        principalColumn: "CAT_ID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}ALLOWED_MEMBERS",
                columns: table => new
                {
                    MEMBER_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    FORUM_ID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_FORUM_ALLOWED_MEMBERS_FORUM_FORUM_FORUM_ID",
                        column: x => x.FORUM_ID,
                        principalTable: $"{_forumTablePrefix}FORUM",
                        principalColumn: "FORUM_ID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}TOPICS",
                columns: table => new
                {
                    TOPIC_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CAT_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    FORUM_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    T_STATUS = table.Column<short>(type: "INTEGER", nullable: false),
                    T_MAIL = table.Column<short>(type: "INTEGER", nullable: false),
                    T_SUBJECT = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    T_MESSAGE = table.Column<string>(type: "TEXT", nullable: false),
                    T_AUTHOR = table.Column<int>(type: "INTEGER", nullable: false),
                    T_REPLIES = table.Column<int>(type: "INTEGER", nullable: false),
                    T_UREPLIES = table.Column<int>(type: "INTEGER", nullable: false),
                    T_VIEW_COUNT = table.Column<int>(type: "INTEGER", nullable: false),
                    T_LAST_POST = table.Column<string>(type: "TEXT", maxLength: 14, nullable: true),
                    T_DATE = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                    LastPoster = table.Column<int>(type: "INTEGER", nullable: true),
                    T_IP = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    T_LAST_POST_AUTHOR = table.Column<int>(type: "INTEGER", nullable: true),
                    T_LAST_POST_REPLY_ID = table.Column<int>(type: "INTEGER", nullable: true),
                    T_ARCHIVE_FLAG = table.Column<int>(type: "INTEGER", nullable: true),
                    T_LAST_EDIT = table.Column<string>(type: "TEXT", maxLength: 14, nullable: true),
                    T_LAST_EDITBY = table.Column<int>(type: "INTEGER", nullable: true),
                    T_STICKY = table.Column<short>(type: "INTEGER", nullable: false),
                    T_SIG = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_TOPICS", x => x.TOPIC_ID);
                    table.ForeignKey(
                        name: "FK_FORUM_TOPICS_FORUM_CATEGORY_CAT_ID",
                        column: x => x.CAT_ID,
                        principalTable: $"{_forumTablePrefix}CATEGORY",
                        principalColumn: "CAT_ID",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_FORUM_TOPICS_FORUM_FORUM_FORUM_ID",
                        column: x => x.FORUM_ID,
                        principalTable: $"{_forumTablePrefix}FORUM",
                        principalColumn: "FORUM_ID",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_FORUM_TOPICS_FORUM_MEMBERS_T_AUTHOR",
                        column: x => x.T_AUTHOR,
                        principalTable: $"{_memberTablePrefix}MEMBERS",
                        principalColumn: "MEMBER_ID",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_FORUM_TOPICS_FORUM_MEMBERS_T_LAST_POST_AUTHOR",
                        column: x => x.T_LAST_POST_AUTHOR,
                        principalTable: $"{_memberTablePrefix}MEMBERS",
                        principalColumn: "MEMBER_ID");
                });

            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}REPLY",
                columns: table => new
                {
                    REPLY_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CAT_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    FORUM_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    TOPIC_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    R_MAIL = table.Column<short>(type: "INTEGER", nullable: false),
                    R_AUTHOR = table.Column<int>(type: "INTEGER", nullable: false),
                    R_MESSAGE = table.Column<string>(type: "TEXT", nullable: false),
                    R_DATE = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                    R_IP = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    R_STATUS = table.Column<short>(type: "INTEGER", nullable: false),
                    R_LAST_EDIT = table.Column<string>(type: "TEXT", maxLength: 14, nullable: true),
                    R_LAST_EDITBY = table.Column<int>(type: "INTEGER", nullable: true),
                    R_SIG = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_REPLY", x => x.REPLY_ID);
                    table.ForeignKey(
                        name: "FK_FORUM_REPLY_FORUM_MEMBERS_R_AUTHOR",
                        column: x => x.R_AUTHOR,
                        principalTable: $"{_memberTablePrefix}MEMBERS",
                        principalColumn: "MEMBER_ID",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_FORUM_REPLY_FORUM_TOPICS_TOPIC_ID",
                        column: x => x.TOPIC_ID,
                        principalTable: $"{_forumTablePrefix}TOPICS",
                        principalColumn: "TOPIC_ID",
                        onDelete: ReferentialAction.NoAction);
                });


            migrationBuilder.CreateIndex(
                name: "IX_FORUM_ALLOWED_MEMBERS_FORUM_ID",
                table: $"{_forumTablePrefix}ALLOWED_MEMBERS",
                column: "FORUM_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_FORUM_CAT_ID",
                table: $"{_forumTablePrefix}FORUM",
                column: "CAT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_REPLY_R_AUTHOR",
                table: $"{_forumTablePrefix}REPLY",
                column: "R_AUTHOR");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_REPLY_TOPIC_ID",
                table: $"{_forumTablePrefix}REPLY",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            SetParameters();
            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}A_REPLY");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}A_TOPICS");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}ALLOWED_MEMBERS");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}BADWORDS");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}CONFIG_NEW");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}GROUP_NAMES");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}GROUPS");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}MODERATOR");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}NAMEFILTER");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}REPLY");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}SUBSCRIPTIONS");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}TOTALS");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}TOPICS");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}FORUM");

            migrationBuilder.DropTable(
                name: $"{_memberTablePrefix}MEMBERS");

            migrationBuilder.DropTable(
                name: $"{_forumTablePrefix}CATEGORY");
        }
    }
}
