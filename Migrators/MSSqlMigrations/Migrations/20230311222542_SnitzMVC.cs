using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Models;
using System;
using System.Configuration;
using System.Reflection;

#nullable disable

namespace Migrations
{
    /// <inheritdoc />
    public partial class SnitzMVC : SnitzMigration
    {

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            SetParameters();

            if (!migrationBuilder.ColumnExists("AspNetUsers", "Discriminator"))
            {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
            }

            if (!migrationBuilder.ColumnExists("AspNetUsers", "IsActive"))
            {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "bit",
                nullable: true);
            }

            if (!migrationBuilder.ColumnExists("AspNetUsers", "IsAdmin"))
            {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "AspNetUsers",
                type: "bit",
                nullable: true);
            }

            if (!migrationBuilder.ColumnExists("AspNetUsers", "MemberId"))
            {
            migrationBuilder.AddColumn<int>(
                name: "MemberId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
            }

            if (!migrationBuilder.ColumnExists("AspNetUsers", "MemberSince"))
            {
            migrationBuilder.AddColumn<DateTime>(
                name: "MemberSince",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
            }

            if (!migrationBuilder.ColumnExists("AspNetUsers", "ProfileImageUrl"))
            {
            migrationBuilder.AddColumn<string>(
                name: "ProfileImageUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
            }

            if (!migrationBuilder.ColumnExists("AspNetUsers", "Rating"))
            {
            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
            }

            if (!migrationBuilder.ColumnExists("AspNetUsers", "UserDescription"))
            {
            migrationBuilder.AddColumn<string>(
                name: "UserDescription",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
            }


            if (!migrationBuilder.TableExists($"{_forumTablePrefix}FORUM"))
            {

                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}A_REPLY",
                    columns: table => new
                    {
                        REPLY_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        CAT_ID = table.Column<int>(type: "int", nullable: false),
                        FORUM_ID = table.Column<int>(type: "int", nullable: false),
                        TOPIC_ID = table.Column<int>(type: "int", nullable: false),
                        R_MAIL = table.Column<short>(type: "smallint", nullable: false),
                        R_AUTHOR = table.Column<int>(type: "int", nullable: false),
                        R_MESSAGE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                        R_DATE = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                        R_IP = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                        R_STATUS = table.Column<short>(type: "smallint", nullable: false),
                        R_LAST_EDIT = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                        R_LAST_EDITBY = table.Column<int>(type: "int", nullable: true),
                        R_SIG = table.Column<short>(type: "smallint", nullable: false),
                        R_RATING = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_A_REPLY", x => x.REPLY_ID);
                    });

                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}A_TOPICS",
                    columns: table => new
                    {
                        TOPIC_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        CAT_ID = table.Column<int>(type: "int", nullable: false),
                        FORUM_ID = table.Column<int>(type: "int", nullable: false),
                        T_STATUS = table.Column<short>(type: "smallint", nullable: false),
                        T_MAIL = table.Column<short>(type: "smallint", nullable: false),
                        T_SUBJECT = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                        T_MESSAGE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                        T_AUTHOR = table.Column<int>(type: "int", nullable: false),
                        T_REPLIES = table.Column<int>(type: "int", nullable: false),
                        T_UREPLIES = table.Column<int>(type: "int", nullable: false),
                        T_VIEW_COUNT = table.Column<int>(type: "int", nullable: false),
                        T_LAST_POST = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                        T_DATE = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                        LastPoster = table.Column<int>(type: "int", nullable: true),
                        T_IP = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                        T_LAST_POST_AUTHOR = table.Column<int>(type: "int", nullable: true),
                        T_LAST_POST_REPLY_ID = table.Column<int>(type: "int", nullable: true),
                        T_ARCHIVE_FLAG = table.Column<int>(type: "int", nullable: true),
                        T_LAST_EDIT = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
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
                    });

                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}BADWORDS",
                    columns: table => new
                    {
                        B_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        B_BADWORD = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                        B_REPLACE = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_BADWORDS", x => x.B_ID);
                    });

                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}CATEGORY",
                    columns: table => new
                    {
                        CAT_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        CAT_STATUS = table.Column<short>(type: "smallint", nullable: true),
                        CAT_NAME = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        CAT_MODERATION = table.Column<int>(type: "int", nullable: true),
                        CAT_SUBSCRIPTION = table.Column<int>(type: "int", nullable: true),
                        CAT_ORDER = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_CATEGORY", x => x.CAT_ID);
                    });

                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}CONFIG_NEW",
                    columns: table => new
                    {
                        ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        C_VARIABLE = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                        C_VALUE = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_CONFIG_NEW", x => x.ID);
                        table.UniqueConstraint("AK_FORUM_CONFIG_NEW_VALUE", x => x.C_VARIABLE);
                    });

                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}GROUP_NAMES",
                    columns: table => new
                    {
                        GROUP_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        GROUP_NAME = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                        GROUP_DESCRIPTION = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                        GROUP_ICON = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                        GROUP_IMAGE = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_GROUP_NAMES", x => x.GROUP_ID);
                    });

                migrationBuilder.CreateTable(
                    name: $"{_memberTablePrefix}MEMBERS",
                    columns: table => new
                    {
                        MEMBER_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        M_NAME = table.Column<string>(type: "nvarchar(75)", maxLength: 75, nullable: false),
                        M_EMAIL = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                        M_TITLE = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                        M_LEVEL = table.Column<short>(type: "smallint", nullable: false),
                        M_STATUS = table.Column<short>(type: "smallint", nullable: false),
                        M_POSTS = table.Column<int>(type: "int", nullable: false),
                        M_DATE = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                        M_FIRSTNAME = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                        M_LASTNAME = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                        M_CITY = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                        M_STATE = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                        M_PHOTO_URL = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                        M_COUNTRY = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                        M_SEX = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                        M_AGE = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                        M_DOB = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                        M_OCCUPATION = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                        M_HOMEPAGE = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                        M_MARSTATUS = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                        M_DEFAULT_VIEW = table.Column<int>(type: "int", nullable: false),
                        M_SIG = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        M_VIEW_SIG = table.Column<short>(type: "smallint", nullable: false),
                        M_SIG_DEFAULT = table.Column<short>(type: "smallint", nullable: false),
                        M_HIDE_EMAIL = table.Column<short>(type: "smallint", nullable: false),
                        M_RECEIVE_EMAIL = table.Column<short>(type: "smallint", nullable: false),
                        M_PMEMAIL = table.Column<int>(type: "int", nullable: false),
                        M_PMRECEIVE = table.Column<int>(type: "int", nullable: false),
                        M_PMSAVESENT = table.Column<short>(type: "smallint", nullable: false),
                        M_LAST_IP = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                        M_IP = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                        M_ALLOWEMAIL = table.Column<short>(type: "smallint", nullable: false),
                        M_SUBSCRIPTION = table.Column<short>(type: "smallint", nullable: false),
                        M_KEY = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                        M_NEWEMAIL = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                        M_PWKEY = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                        M_SHA256 = table.Column<short>(type: "smallint", nullable: true, defaultValue: 1),
                        M_LASTACTIVITY = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                        M_AIM = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                        M_ICQ = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                        M_MSN = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                        M_YAHOO = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                        M_HOBBIES = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        M_LNEWS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        M_QUOTE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        M_BIO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        M_LINK1 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                        M_LINK2 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                        M_LastLogin = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                        M_LASTPOSTDATE = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                        M_PRIVATEPROFILE = table.Column<short>(type: "smallint", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_MEMBERS", x => x.MEMBER_ID);
                    });

                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}NAMEFILTER",
                    columns: table => new
                    {
                        N_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        N_NAME = table.Column<string>(type: "nvarchar(75)", maxLength: 75, nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_NAMEFILTER", x => x.N_ID);
                    });

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
                        M_OUTBOX = table.Column<short>(type: "smallint", nullable: false),
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
                        BL_BLOCKED_NAME = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
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
                    name: $"{_forumTablePrefix}SUBSCRIPTIONS",
                    columns: table => new
                    {
                        SUBSCRIPTION_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        MEMBER_ID = table.Column<int>(type: "int", nullable: false),
                        CAT_ID = table.Column<int>(type: "int", nullable: false),
                        FORUM_ID = table.Column<int>(type: "int", nullable: false),
                        TOPIC_ID = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_SUBSCRIPTIONS", x => x.SUBSCRIPTION_ID);
                    });

                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}TOTALS",
                    columns: table => new
                    {
                        COUNT_ID = table.Column<short>(type: "smallint", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        P_COUNT = table.Column<int>(type: "int", nullable: false),
                        P_A_COUNT = table.Column<int>(type: "int", nullable: false),
                        T_COUNT = table.Column<int>(type: "int", nullable: false),
                        T_A_COUNT = table.Column<int>(type: "int", nullable: false),
                        U_COUNT = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_TOTALS", x => x.COUNT_ID);
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
                    name: "TOPIC_RATINGS",
                    columns: table => new
                    {
                        RATING = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        RATINGS_BYMEMBER_ID = table.Column<int>(type: "int", nullable: false),
                        RATINGS_TOPIC_ID = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_TOPIC_RATINGS", x => x.RATING);
                    });

                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}FORUM",
                    columns: table => new
                    {
                        FORUM_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        CAT_ID = table.Column<int>(type: "int", nullable: false),
                        F_STATUS = table.Column<short>(type: "smallint", nullable: false),
                        F_MAIL = table.Column<short>(type: "smallint", nullable: false),
                        F_SUBJECT = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                        F_URL = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                        F_DESCRIPTION = table.Column<string>(type: "nvarchar(max)", nullable: false),
                        F_TOPICS = table.Column<int>(type: "int", nullable: false),
                        F_COUNT = table.Column<int>(type: "int", nullable: false),
                        F_LAST_POST = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                        F_PASSWORD_NEW = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        F_PRIVATEFORUMS = table.Column<int>(type: "int", nullable: false),
                        F_TYPE = table.Column<short>(type: "smallint", nullable: false),
                        F_IP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        F_LAST_POST_AUTHOR = table.Column<int>(type: "int", nullable: true),
                        F_LAST_POST_TOPIC_ID = table.Column<int>(type: "int", nullable: true),
                        F_LAST_POST_REPLY_ID = table.Column<int>(type: "int", nullable: true),
                        F_A_TOPICS = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                        F_A_COUNT = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                        F_MODERATION = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                        F_SUBSCRIPTION = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                        F_ORDER = table.Column<int>(type: "int", nullable: false, defaultValue: 99),
                        F_DEFAULTDAYS = table.Column<int>(type: "int", nullable: false),
                        F_COUNT_M_POSTS = table.Column<short>(type: "smallint", nullable: false, defaultValue: 1),
                        F_L_ARCHIVE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        F_ARCHIVE_SCHED = table.Column<int>(type: "int", nullable: false, defaultValue: 60),
                        F_L_DELETE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        F_DELETE_SCHED = table.Column<int>(type: "int", nullable: false, defaultValue: 365),
                        F_POLLS = table.Column<int>(type: "int", nullable: true),
                        F_RATING = table.Column<short>(type: "smallint", nullable: false, defaultValue: 0),
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
                    });

                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}GROUPS",
                    columns: table => new
                    {
                        GROUP_KEY = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
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
                    });

                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}ALLOWED_MEMBERS",
                    columns: table => new
                    {
                        MEMBER_ID = table.Column<int>(type: "int", nullable: false),
                        FORUM_ID = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.ForeignKey(
                            name: "FK_FORUM_ALLOWED_MEMBERS_FORUM_FORUM_FORUM_ID",
                            column: x => x.FORUM_ID,
                            principalTable: $"{_forumTablePrefix}FORUM",
                            principalColumn: "FORUM_ID",
                            onDelete: ReferentialAction.Cascade);
                    });

                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}MODERATOR",
                    columns: table => new
                    {
                        MOD_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
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
                    });

                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}TOPICS",
                    columns: table => new
                    {
                        TOPIC_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        CAT_ID = table.Column<int>(type: "int", nullable: false),
                        FORUM_ID = table.Column<int>(type: "int", nullable: false),
                        T_STATUS = table.Column<short>(type: "smallint", nullable: false),
                        T_MAIL = table.Column<short>(type: "smallint", nullable: false),
                        T_SUBJECT = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                        T_MESSAGE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                        T_AUTHOR = table.Column<int>(type: "int", nullable: false),
                        T_REPLIES = table.Column<int>(type: "int", nullable: false),
                        T_UREPLIES = table.Column<int>(type: "int", nullable: false),
                        T_VIEW_COUNT = table.Column<int>(type: "int", nullable: false),
                        T_LAST_POST = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                        T_DATE = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                        LastPoster = table.Column<int>(type: "int", nullable: true),
                        T_IP = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                        T_LAST_POST_AUTHOR = table.Column<int>(type: "int", nullable: true),
                        T_LAST_POST_REPLY_ID = table.Column<int>(type: "int", nullable: true),
                        T_ARCHIVE_FLAG = table.Column<int>(type: "int", nullable: true),
                        T_LAST_EDIT = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
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
                    });

                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}REPLY",
                    columns: table => new
                    {
                        REPLY_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        CAT_ID = table.Column<int>(type: "int", nullable: false),
                        FORUM_ID = table.Column<int>(type: "int", nullable: false),
                        TOPIC_ID = table.Column<int>(type: "int", nullable: false),
                        R_MAIL = table.Column<short>(type: "smallint", nullable: false),
                        R_AUTHOR = table.Column<int>(type: "int", nullable: false),
                        R_MESSAGE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                        R_DATE = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                        R_IP = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                        R_STATUS = table.Column<short>(type: "smallint", nullable: false),
                        R_LAST_EDIT = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                        R_LAST_EDITBY = table.Column<int>(type: "int", nullable: true),
                        R_SIG = table.Column<short>(type: "smallint", nullable: false),
                        R_RATING = table.Column<int>(type: "int", nullable: false)
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
                    name: "IX_FORUM_REPLY_R_AUTHOR",
                    table: $"{_forumTablePrefix}REPLY",
                    column: "R_AUTHOR");

                migrationBuilder.CreateIndex(
                    name: "IX_FORUM_REPLY_TOPIC_ID",
                    table: $"{_forumTablePrefix}EPLY",
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
            if (migrationBuilder.IsSqlite())
            {

                if (!migrationBuilder.IndexExists("SELECT * FROM sqlite_master WHERE type= 'index' and tbl_name = 'AspNetUsers' and name = 'IX_AspNetUsers_MemberId'"))
                {
                    migrationBuilder.CreateIndex(
                        name: "IX_AspNetUsers_MemberId",
                        table: "AspNetUsers",
                        column: "MemberId");
                }

            }
            else
            {
                if (!migrationBuilder.IndexExists("SELECT COUNT(*) FROM sys.indexes WHERE object_id = OBJECT_ID('AspNetUsers') AND name='IX_AspNetUsers_MemberId'"))
                {
                    migrationBuilder.CreateIndex(
                        name: "IX_AspNetUsers_MemberId",
                        table: "AspNetUsers",
                        column: "MemberId");
                }
                if (!migrationBuilder.IndexExists("SELECT COUNT(*) FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_AspNetUsers_FORUM_MEMBERS_MemberId') AND parent_object_id = OBJECT_ID(N'AspNetUsers')"))
                {
                    migrationBuilder.AddForeignKey(
                        name: "FK_AspNetUsers_FORUM_MEMBERS_MemberId",
                        table: "AspNetUsers",
                        column: "MemberId",
                        principalTable: $"{_memberTablePrefix}MEMBERS",
                        principalColumn: "MEMBER_ID",
                        onDelete: ReferentialAction.Cascade);
                }
            }




        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_FORUM_MEMBERS_MemberId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "FORUM_A_REPLY");

            migrationBuilder.DropTable(
                name: "FORUM_A_TOPICS");

            migrationBuilder.DropTable(
                name: "FORUM_ALLOWED_MEMBERS");

            migrationBuilder.DropTable(
                name: "FORUM_BADWORDS");

            migrationBuilder.DropTable(
                name: "FORUM_CONFIG_NEW");

            migrationBuilder.DropTable(
                name: "FORUM_GROUPS");

            migrationBuilder.DropTable(
                name: "FORUM_MODERATOR");

            migrationBuilder.DropTable(
                name: "FORUM_NAMEFILTER");

            migrationBuilder.DropTable(
                name: "FORUM_PM");

            migrationBuilder.DropTable(
                name: "FORUM_PM_BLOCKLIST");

            migrationBuilder.DropTable(
                name: "FORUM_RANKING");

            migrationBuilder.DropTable(
                name: "FORUM_REPLY");

            migrationBuilder.DropTable(
                name: "FORUM_SUBSCRIPTIONS");

            migrationBuilder.DropTable(
                name: "FORUM_TOTALS");

            migrationBuilder.DropTable(
                name: "LANGUAGE_RES");

            migrationBuilder.DropTable(
                name: "TOPIC_RATINGS");

            migrationBuilder.DropTable(
                name: "FORUM_GROUP_NAMES");

            migrationBuilder.DropTable(
                name: "FORUM_TOPICS");

            migrationBuilder.DropTable(
                name: "FORUM_FORUM");

            migrationBuilder.DropTable(
                name: "FORUM_MEMBERS");

            migrationBuilder.DropTable(
                name: "FORUM_CATEGORY");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_MemberId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MemberSince",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProfileImageUrl",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UserDescription",
                table: "AspNetUsers");
        }
    }
}
