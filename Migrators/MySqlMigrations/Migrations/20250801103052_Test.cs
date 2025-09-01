using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MySqlMigrations.Migrations
{
    /// <inheritdoc />
    public partial class Test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

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
                name: "FORUM_BADWORDS",
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



            migrationBuilder.CreateTable(
                name: "FORUM_CATEGORY",
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
                name: "FORUM_CONFIG_NEW",
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

            migrationBuilder.CreateTable(
                name: "FORUM_GROUP_NAMES",
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
                name: "FORUM_MEMBERS",
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
                name: "FORUM_NAMEFILTER",
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
                name: "FORUM_ORG_GROUP",
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

            migrationBuilder.CreateTable(
                name: "FORUM_PM_BLOCKLIST",
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

            migrationBuilder.CreateTable(
                name: "FORUM_RANKING",
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

            migrationBuilder.CreateTable(
                name: "FORUM_SPAM_MAIL",
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
                name: "FORUM_TOTALS",
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

            migrationBuilder.CreateTable(
                name: "webpages_Membership",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreateDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Password = table.Column<string>(type: "varchar(75)", maxLength: 75, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_webpages_Membership", x => x.UserId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "webpages_Roles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_webpages_Roles", x => x.RoleId);
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

            migrationBuilder.CreateTable(
                name: "FORUM_GROUPS",
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
                        principalTable: "FORUM_CATEGORY",
                        principalColumn: "CAT_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FORUM_GROUPS_FORUM_GROUP_NAMES_GROUP_ID",
                        column: x => x.GROUP_ID,
                        principalTable: "FORUM_GROUP_NAMES",
                        principalColumn: "GROUP_ID");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
                        name: "FK_AspNetUsers_FORUM_MEMBERS_MemberId",
                        column: x => x.MemberId,
                        principalTable: "FORUM_MEMBERS",
                        principalColumn: "MEMBER_ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FORUM_PM",
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
                        principalTable: "FORUM_MEMBERS",
                        principalColumn: "MEMBER_ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");



            migrationBuilder.CreateTable(
                name: "webpages_UsersInRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_webpages_UsersInRoles_FORUM_MEMBERS_UserId",
                        column: x => x.UserId,
                        principalTable: "FORUM_MEMBERS",
                        principalColumn: "MEMBER_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_webpages_UsersInRoles_webpages_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "webpages_Roles",
                        principalColumn: "RoleId",
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



            migrationBuilder.CreateTable(
                name: "FORUM_A_REPLY",
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
                        principalTable: "FORUM_MEMBERS",
                        principalColumn: "MEMBER_ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FORUM_A_TOPICS",
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
                        principalTable: "FORUM_CATEGORY",
                        principalColumn: "CAT_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FORUM_A_TOPICS_FORUM_MEMBERS_T_AUTHOR",
                        column: x => x.T_AUTHOR,
                        principalTable: "FORUM_MEMBERS",
                        principalColumn: "MEMBER_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FORUM_A_TOPICS_FORUM_MEMBERS_T_LAST_POST_AUTHOR",
                        column: x => x.T_LAST_POST_AUTHOR,
                        principalTable: "FORUM_MEMBERS",
                        principalColumn: "MEMBER_ID");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FORUM_ALLOWED_MEMBERS",
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
                        principalTable: "FORUM_MEMBERS",
                        principalColumn: "MEMBER_ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FORUM_BOOKMARKS",
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
                        principalTable: "FORUM_MEMBERS",
                        principalColumn: "MEMBER_ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FORUM_FORUM",
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
                        principalTable: "FORUM_CATEGORY",
                        principalColumn: "CAT_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FORUM_FORUM_FORUM_MEMBERS_F_LAST_POST_AUTHOR",
                        column: x => x.F_LAST_POST_AUTHOR,
                        principalTable: "FORUM_MEMBERS",
                        principalColumn: "MEMBER_ID");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FORUM_MODERATOR",
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
                        principalTable: "FORUM_FORUM",
                        principalColumn: "FORUM_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FORUM_MODERATOR_FORUM_MEMBERS_MEMBER_ID",
                        column: x => x.MEMBER_ID,
                        principalTable: "FORUM_MEMBERS",
                        principalColumn: "MEMBER_ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FORUM_TOPICS",
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
                    T_ANSWERED = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_TOPICS", x => x.TOPIC_ID);
                    table.ForeignKey(
                        name: "FK_FORUM_TOPICS_FORUM_CATEGORY_CAT_ID",
                        column: x => x.CAT_ID,
                        principalTable: "FORUM_CATEGORY",
                        principalColumn: "CAT_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FORUM_TOPICS_FORUM_FORUM_FORUM_ID",
                        column: x => x.FORUM_ID,
                        principalTable: "FORUM_FORUM",
                        principalColumn: "FORUM_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FORUM_TOPICS_FORUM_MEMBERS_T_AUTHOR",
                        column: x => x.T_AUTHOR,
                        principalTable: "FORUM_MEMBERS",
                        principalColumn: "MEMBER_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FORUM_TOPICS_FORUM_MEMBERS_T_LAST_POST_AUTHOR",
                        column: x => x.T_LAST_POST_AUTHOR,
                        principalTable: "FORUM_MEMBERS",
                        principalColumn: "MEMBER_ID");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FORUM_POLLS",
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
                        principalTable: "FORUM_TOPICS",
                        principalColumn: "TOPIC_ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FORUM_REPLY",
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
                        principalTable: "FORUM_MEMBERS",
                        principalColumn: "MEMBER_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FORUM_REPLY_FORUM_TOPICS_TOPIC_ID",
                        column: x => x.TOPIC_ID,
                        principalTable: "FORUM_TOPICS",
                        principalColumn: "TOPIC_ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FORUM_SUBSCRIPTIONS",
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
                        principalTable: "FORUM_CATEGORY",
                        principalColumn: "CAT_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FORUM_SUBSCRIPTIONS_FORUM_FORUM_FORUM_ID",
                        column: x => x.FORUM_ID,
                        principalTable: "FORUM_FORUM",
                        principalColumn: "FORUM_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FORUM_SUBSCRIPTIONS_FORUM_MEMBERS_MEMBER_ID",
                        column: x => x.MEMBER_ID,
                        principalTable: "FORUM_MEMBERS",
                        principalColumn: "MEMBER_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FORUM_SUBSCRIPTIONS_FORUM_TOPICS_TOPIC_ID",
                        column: x => x.TOPIC_ID,
                        principalTable: "FORUM_TOPICS",
                        principalColumn: "TOPIC_ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FORUM_POLL_ANSWERS",
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
                        principalTable: "FORUM_POLLS",
                        principalColumn: "POLL_ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FORUM_POLL_VOTES",
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
                        principalTable: "FORUM_POLLS",
                        principalColumn: "POLL_ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
                table: "FORUM_A_REPLY",
                column: "R_AUTHOR");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_A_REPLY_TOPIC_ID",
                table: "FORUM_A_REPLY",
                column: "TOPIC_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_A_TOPICS_CAT_ID",
                table: "FORUM_A_TOPICS",
                column: "CAT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_A_TOPICS_FORUM_ID",
                table: "FORUM_A_TOPICS",
                column: "FORUM_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_A_TOPICS_T_AUTHOR",
                table: "FORUM_A_TOPICS",
                column: "T_AUTHOR");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_A_TOPICS_T_LAST_POST_AUTHOR",
                table: "FORUM_A_TOPICS",
                column: "T_LAST_POST_AUTHOR");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_ALLOWED_MEMBERS_FORUM_ID",
                table: "FORUM_ALLOWED_MEMBERS",
                column: "FORUM_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_BOOKMARKS_B_MEMBERID",
                table: "FORUM_BOOKMARKS",
                column: "B_MEMBERID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_BOOKMARKS_B_TOPICID",
                table: "FORUM_BOOKMARKS",
                column: "B_TOPICID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_CONFIG_NEW_C_VARIABLE",
                table: "FORUM_CONFIG_NEW",
                column: "C_VARIABLE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_FORUM_CAT_ID",
                table: "FORUM_FORUM",
                column: "CAT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_FORUM_F_LAST_POST_AUTHOR",
                table: "FORUM_FORUM",
                column: "F_LAST_POST_AUTHOR");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_FORUM_F_LAST_POST_TOPIC_ID",
                table: "FORUM_FORUM",
                column: "F_LAST_POST_TOPIC_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_GROUPS_GROUP_CATID",
                table: "FORUM_GROUPS",
                column: "GROUP_CATID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_GROUPS_GROUP_ID",
                table: "FORUM_GROUPS",
                column: "GROUP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_MODERATOR_FORUM_ID",
                table: "FORUM_MODERATOR",
                column: "FORUM_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_MODERATOR_MEMBER_ID",
                table: "FORUM_MODERATOR",
                column: "MEMBER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_PM_M_FROM",
                table: "FORUM_PM",
                column: "M_FROM");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_POLL_ANSWERS_POLL_ID",
                table: "FORUM_POLL_ANSWERS",
                column: "POLL_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_POLL_VOTES_POLL_ID",
                table: "FORUM_POLL_VOTES",
                column: "POLL_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_POLLS_TOPIC_ID",
                table: "FORUM_POLLS",
                column: "TOPIC_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_REPLY_R_AUTHOR",
                table: "FORUM_REPLY",
                column: "R_AUTHOR");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_REPLY_TOPIC_ID",
                table: "FORUM_REPLY",
                column: "TOPIC_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_SUBSCRIPTIONS_CAT_ID",
                table: "FORUM_SUBSCRIPTIONS",
                column: "CAT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_SUBSCRIPTIONS_FORUM_ID",
                table: "FORUM_SUBSCRIPTIONS",
                column: "FORUM_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_SUBSCRIPTIONS_MEMBER_ID",
                table: "FORUM_SUBSCRIPTIONS",
                column: "MEMBER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_SUBSCRIPTIONS_TOPIC_ID",
                table: "FORUM_SUBSCRIPTIONS",
                column: "TOPIC_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_TOPICS_CAT_ID",
                table: "FORUM_TOPICS",
                column: "CAT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_TOPICS_FORUM_ID",
                table: "FORUM_TOPICS",
                column: "FORUM_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_TOPICS_T_AUTHOR",
                table: "FORUM_TOPICS",
                column: "T_AUTHOR");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_TOPICS_T_LAST_POST_AUTHOR",
                table: "FORUM_TOPICS",
                column: "T_LAST_POST_AUTHOR");

            migrationBuilder.CreateIndex(
                name: "IX_webpages_UsersInRoles_RoleId",
                table: "webpages_UsersInRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_webpages_UsersInRoles_UserId",
                table: "webpages_UsersInRoles",
                column: "UserId");


            migrationBuilder.AddForeignKey(
                name: "FK_FORUM_A_REPLY_FORUM_A_TOPICS_TOPIC_ID",
                table: "FORUM_A_REPLY",
                column: "TOPIC_ID",
                principalTable: "FORUM_A_TOPICS",
                principalColumn: "TOPIC_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FORUM_A_TOPICS_FORUM_FORUM_FORUM_ID",
                table: "FORUM_A_TOPICS",
                column: "FORUM_ID",
                principalTable: "FORUM_FORUM",
                principalColumn: "FORUM_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FORUM_ALLOWED_MEMBERS_FORUM_FORUM_FORUM_ID",
                table: "FORUM_ALLOWED_MEMBERS",
                column: "FORUM_ID",
                principalTable: "FORUM_FORUM",
                principalColumn: "FORUM_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FORUM_BOOKMARKS_FORUM_TOPICS_B_TOPICID",
                table: "FORUM_BOOKMARKS",
                column: "B_TOPICID",
                principalTable: "FORUM_TOPICS",
                principalColumn: "TOPIC_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FORUM_FORUM_FORUM_TOPICS_F_LAST_POST_TOPIC_ID",
                table: "FORUM_FORUM",
                column: "F_LAST_POST_TOPIC_ID",
                principalTable: "FORUM_TOPICS",
                principalColumn: "TOPIC_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FORUM_FORUM_FORUM_MEMBERS_F_LAST_POST_AUTHOR",
                table: "FORUM_FORUM");

            migrationBuilder.DropForeignKey(
                name: "FK_FORUM_TOPICS_FORUM_MEMBERS_T_AUTHOR",
                table: "FORUM_TOPICS");

            migrationBuilder.DropForeignKey(
                name: "FK_FORUM_TOPICS_FORUM_MEMBERS_T_LAST_POST_AUTHOR",
                table: "FORUM_TOPICS");

            migrationBuilder.DropForeignKey(
                name: "FK_FORUM_FORUM_FORUM_TOPICS_F_LAST_POST_TOPIC_ID",
                table: "FORUM_FORUM");

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
                name: "FORUM_A_REPLY");

            migrationBuilder.DropTable(
                name: "FORUM_ALLOWED_MEMBERS");

            migrationBuilder.DropTable(
                name: "FORUM_BADWORDS");

            migrationBuilder.DropTable(
                name: "FORUM_BOOKMARKS");

            migrationBuilder.DropTable(
                name: "FORUM_CONFIG_NEW");

            migrationBuilder.DropTable(
                name: "FORUM_GROUPS");

            migrationBuilder.DropTable(
                name: "FORUM_IMAGES");

            migrationBuilder.DropTable(
                name: "FORUM_MODERATOR");

            migrationBuilder.DropTable(
                name: "FORUM_NAMEFILTER");

            migrationBuilder.DropTable(
                name: "FORUM_PM");

            migrationBuilder.DropTable(
                name: "FORUM_PM_BLOCKLIST");

            migrationBuilder.DropTable(
                name: "FORUM_POLL_ANSWERS");

            migrationBuilder.DropTable(
                name: "FORUM_POLL_VOTES");

            migrationBuilder.DropTable(
                name: "FORUM_RANKING");

            migrationBuilder.DropTable(
                name: "FORUM_REPLY");

            migrationBuilder.DropTable(
                name: "FORUM_SPAM_MAIL");

            migrationBuilder.DropTable(
                name: "FORUM_SUBSCRIPTIONS");

            migrationBuilder.DropTable(
                name: "FORUM_TOTALS");

            migrationBuilder.DropTable(
                name: "LANGUAGE_RES");

            migrationBuilder.DropTable(
                name: "TOPIC_RATINGS");

            migrationBuilder.DropTable(
                name: "webpages_Membership");

            migrationBuilder.DropTable(
                name: "webpages_UsersInRoles");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "FORUM_A_TOPICS");

            migrationBuilder.DropTable(
                name: "FORUM_GROUP_NAMES");

            migrationBuilder.DropTable(
                name: "FORUM_IMAGE_CAT");

            migrationBuilder.DropTable(
                name: "FORUM_ORG_GROUP");

            migrationBuilder.DropTable(
                name: "FORUM_POLLS");

            migrationBuilder.DropTable(
                name: "webpages_Roles");

            migrationBuilder.DropTable(
                name: "FORUM_MEMBERS");

            migrationBuilder.DropTable(
                name: "FORUM_TOPICS");

            migrationBuilder.DropTable(
                name: "FORUM_FORUM");

            migrationBuilder.DropTable(
                name: "FORUM_CATEGORY");
        }
    }
}
