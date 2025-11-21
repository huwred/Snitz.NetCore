using Microsoft.EntityFrameworkCore.Migrations;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Models;

#nullable disable

namespace Migrations
{

    /// <inheritdoc />
    public partial class InitialCreate : SnitzMigration
    {

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            if (!migrationBuilder.TableExists("AspNetRoles"))
            {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                });
            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

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
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");
            }


            if (!migrationBuilder.IndexExists("SELECT  schema_id FROM    sys.objects  WHERE   object_id = OBJECT_ID(N'snitz_updateCounts') AND type IN ( N'P', N'PC' )"))
            {
            migrationBuilder.Sql(@"
                /****** Object:  StoredProcedure [dbo].[snitz_updateCounts]    Script Date: 07/08/2025 12:36:16 ******/
                SET ANSI_NULLS OFF
                GO
                SET QUOTED_IDENTIFIER OFF
                GO
                CREATE PROCEDURE [dbo].[snitz_updateCounts]
                AS
                BEGIN
	                -- SET NOCOUNT ON added to prevent extra result sets from
	                -- interfering with SELECT statements.
	                SET NOCOUNT ON;

                /* Update Forum Topic Counts */

                CREATE TABLE #F_T_COUNT (
                    FORUM_ID int,
                    T_COUNT int
                );

                UPDATE FORUM_FORUM SET F_TOPICS = 0;

                INSERT INTO #F_T_COUNT
                    SELECT FORUM_ID, COUNT(FORUM_ID) FROM FORUM_TOPICS WHERE T_STATUS<=1 GROUP By FORUM_ID ;

                UPDATE FORUM_FORUM SET F_TOPICS = T_COUNT FROM FORUM_FORUM F INNER JOIN #F_T_COUNT T ON T.FORUM_ID=F.FORUM_ID;
 
                /* Update Forum Archived Topics Count  */

                DELETE FROM #F_T_COUNT;

                UPDATE FORUM_FORUM SET F_A_TOPICS = 0;

                INSERT INTO #F_T_COUNT
                    SELECT FORUM_ID, COUNT(FORUM_ID) FROM FORUM_A_TOPICS WHERE T_STATUS<=1 GROUP By FORUM_ID ;

                UPDATE FORUM_FORUM SET F_A_TOPICS = T_COUNT FROM FORUM_FORUM F INNER JOIN #F_T_COUNT T ON T.FORUM_ID=F.FORUM_ID;

                DROP TABLE #F_T_COUNT;

                /* Update Topic Replies Counts now */

                CREATE TABLE #T_R_COUNT (
                    TOPIC_ID int,
                    R_COUNT int
                );

                INSERT INTO #T_R_COUNT 
                    SELECT TOPIC_ID, COUNT(REPLY_ID) FROM FORUM_REPLY WHERE R_STATUS<=1 GROUP BY TOPIC_ID ;

                UPDATE FORUM_TOPICS SET T_UREPLIES = 0 WHERE T_STATUS<=1;

                UPDATE FORUM_TOPICS SET T_REPLIES = R_COUNT  FROM FORUM_TOPICS T INNER JOIN #T_R_COUNT TR ON T.TOPIC_ID = TR.TOPIC_ID
                    WHERE T.T_STATUS<=1;

                /* Update Archived Topics Replies Count Now */

                DELETE FROM #T_R_COUNT;

                INSERT INTO #T_R_COUNT 
                    SELECT TOPIC_ID, COUNT(REPLY_ID) FROM FORUM_A_REPLY GROUP BY TOPIC_ID;

                UPDATE FORUM_A_TOPICS SET T_UREPLIES = 0 WHERE T_STATUS<=1;

                UPDATE FORUM_A_TOPICS SET T_REPLIES = R_COUNT FROM FORUM_A_TOPICS T INNER JOIN #T_R_COUNT TR ON T.TOPIC_ID = TR.TOPIC_ID;

                DROP TABLE #T_R_COUNT;

                /* Update Last post Date */ 

                CREATE TABLE #T_POST_DATA (
                    TOPIC_ID int,
                    LAST_POST nvarchar(14)
                );

                INSERT INTO #T_POST_DATA
                    SELECT TOPIC_ID, MAX(R_DATE) FROM FORUM_REPLY WHERE R_STATUS<=1 GROUP BY TOPIC_ID;

                UPDATE FORUM_TOPICS SET T_LAST_POST = LAST_POST FROM FORUM_TOPICS INNER JOIN #T_POST_DATA PT  ON FORUM_TOPICS.TOPIC_ID = PT.TOPIC_ID;

                DELETE FROM #T_POST_DATA;

                UPDATE FORUM_TOPICS SET T_LAST_POST=T_DATE, T_LAST_POST_AUTHOR=T_AUTHOR WHERE T_REPLIES=0;

                /* Update Last post Date */ 

                INSERT INTO #T_POST_DATA
                    SELECT TOPIC_ID, MAX(R_DATE) FROM FORUM_A_REPLY GROUP BY TOPIC_ID;

                UPDATE FORUM_A_TOPICS SET T_LAST_POST = LAST_POST FROM FORUM_A_TOPICS T INNER JOIN #T_POST_DATA TP  ON T.TOPIC_ID = TP.TOPIC_ID;

                UPDATE FORUM_A_TOPICS SET T_LAST_POST=T_DATE, T_LAST_POST_AUTHOR=T_AUTHOR WHERE T_REPLIES=0;

                DROP TABLE #T_POST_DATA;
                /* Now find the reply ID for the posts that have more than 0 replies */

                CREATE TABLE #T_L_REPLY_ID (
                    TOPIC_ID int,
                    REPLY_ID int
                );

                CREATE TABLE #T_L_A_REPLY_ID (
                    TOPIC_ID int,
                    REPLY_ID int
                );

                INSERT INTO #T_L_REPLY_ID
                    SELECT T.TOPIC_ID, MAX(REPLY_ID) FROM FORUM_REPLY R INNER JOIN FORUM_TOPICS T ON R.TOPIC_ID=T.TOPIC_ID
                        WHERE T.T_LAST_POST=R_DATE AND T_STATUS<=1 GROUP BY T.TOPIC_ID;

                UPDATE FORUM_TOPICS SET T_LAST_POST_REPLY_ID = REPLY_ID FROM FORUM_TOPICS T LEFT JOIN #T_L_REPLY_ID TL ON TL.TOPIC_ID = T.TOPIC_ID;

                /*DELETE FROM #T_L_REPLY_ID; This line should not be here*/

                /* Now find the reply ID for the posts that have more than 0 replies in archived topics */

                INSERT INTO #T_L_A_REPLY_ID
                    SELECT T.TOPIC_ID, MAX(REPLY_ID) FROM FORUM_A_REPLY R INNER JOIN FORUM_A_TOPICS T ON R.TOPIC_ID=T.TOPIC_ID
                        WHERE T.T_LAST_POST=R_DATE GROUP BY T.TOPIC_ID;

                /* Now found the author ID for the last reply */

                CREATE TABLE #T_L_REPLY_AUTHOR(
                    TOPIC_ID int,
                    AUTHOR int
                );

                INSERT INTO #T_L_REPLY_AUTHOR
                    SELECT T.TOPIC_ID, R.R_AUTHOR FROM FORUM_TOPICS T INNER JOIN FORUM_REPLY R ON T.TOPIC_ID=R.TOPIC_ID
                        INNER JOIN #T_L_REPLY_ID ON T.TOPIC_ID= #T_L_REPLY_ID.TOPIC_ID 
                    WHERE #T_L_REPLY_ID.REPLY_ID = R.REPLY_ID AND T_STATUS<=1;

                UPDATE FORUM_TOPICS SET T_LAST_POST_AUTHOR = AUTHOR FROM FORUM_TOPICS T INNER JOIN #T_L_REPLY_AUTHOR TL ON TL.TOPIC_ID = T.TOPIC_ID;

                DELETE FROM #T_L_REPLY_AUTHOR;

                INSERT INTO #T_L_REPLY_AUTHOR
                    SELECT T.TOPIC_ID, R.R_AUTHOR FROM FORUM_A_TOPICS T INNER JOIN FORUM_A_REPLY R ON T.TOPIC_ID=R.TOPIC_ID
                        INNER JOIN #T_L_A_REPLY_ID ON T.TOPIC_ID = #T_L_A_REPLY_ID.TOPIC_ID
                    WHERE #T_L_A_REPLY_ID.REPLY_ID = R.REPLY_ID;

                UPDATE FORUM_A_TOPICS SET T_LAST_POST_AUTHOR = AUTHOR FROM FORUM_A_TOPICS T INNER JOIN #T_L_REPLY_AUTHOR TL ON TL.TOPIC_ID = T.TOPIC_ID;

                DROP TABLE #T_L_REPLY_AUTHOR;

                DROP TABLE #T_L_REPLY_ID;

                DROP TABLE #T_L_A_REPLY_ID;

                /* Now to step 4 */
                /* Count replies per forum */

                CREATE TABLE #F_R_COUNT (
                    FORUM_ID int,
                    R_COUNT int
                );

                INSERT INTO #F_R_COUNT 
                    SELECT R.FORUM_ID, COUNT(REPLY_ID) FROM FORUM_TOPICS T INNER JOIN FORUM_REPLY R On T.TOPIC_ID=R.TOPIC_ID
                        WHERE T.T_STATUS<=1 AND R_STATUS<=1 GROUP By R.FORUM_ID;

                UPDATE FORUM_FORUM SET F_COUNT=F_TOPICS WHERE F_TYPE<>1;

                UPDATE FORUM_FORUM SET F_COUNT = F_COUNT + R_COUNT FROM FORUM_FORUM F INNER JOIN #F_R_COUNT FR ON F.FORUM_ID = FR.FORUM_ID;

                DELETE FROM #F_R_COUNT;

                INSERT INTO #F_R_COUNT 
                    SELECT R.FORUM_ID, COUNT(REPLY_ID) FROM FORUM_A_TOPICS T INNER JOIN FORUM_A_REPLY R On T.TOPIC_ID=R.TOPIC_ID GROUP By R.FORUM_ID;

                UPDATE FORUM_FORUM SET F_A_COUNT=F_A_TOPICS WHERE F_TYPE<>1;

                UPDATE FORUM_FORUM SET F_A_COUNT = F_A_COUNT + R_COUNT FROM FORUM_FORUM F INNER JOIN #F_R_COUNT FR ON F.FORUM_ID = FR.FORUM_ID
 
                DROP TABLE  #F_R_COUNT;

                /* Update Last Post Per Forum */

                CREATE TABLE #F_POST_DATA (
                    FORUM_ID int,
                    LAST_POST varchar(50)
                );


                INSERT INTO #F_POST_DATA
                    SELECT FORUM_ID, MAX(T_LAST_POST) FROM FORUM_TOPICS WHERE T_STATUS<=1 GROUP BY FORUM_ID;

                UPDATE FORUM_FORUM SET F_LAST_POST = LAST_POST FROM FORUM_FORUM F LEFT JOIN #F_POST_DATA FP  ON F.FORUM_ID = FP.FORUM_ID;

                DROP TABLE #F_POST_DATA;

                /* Update Last Post TOPIC_ID */

                CREATE TABLE #F_TOPIC_ID (
                    FORUM_ID int,
                    TOPIC_ID int
                );

                INSERT INTO #F_TOPIC_ID
                    SELECT F.FORUM_ID, MAX(T.TOPIC_ID) FROM FORUM_FORUM F INNER JOIN FORUM_TOPICS T On F.FORUM_ID=T.FORUM_ID
                    WHERE F.F_LAST_POST = T.T_LAST_POST and T.T_STATUS<=1 GROUP BY F.FORUM_ID;

                UPDATE FORUM_FORUM SET F_LAST_POST_TOPIC_ID = TOPIC_ID FROM FORUM_FORUM F LEFT JOIN #F_TOPIC_ID FT ON F.FORUM_ID = FT.FORUM_ID;
   
                /* Now Update for Author ID */
                UPDATE FORUM_FORUM SET F_LAST_POST_AUTHOR=NULL
                UPDATE FORUM_FORUM SET F_LAST_POST_AUTHOR=T_LAST_POST_AUTHOR,F_LAST_POST_REPLY_ID=T_LAST_POST_REPLY_ID 
                FROM ((FORUM_FORUM F INNER JOIN #F_TOPIC_ID ON F.FORUM_ID=#F_TOPIC_ID.FORUM_ID) LEFT JOIN FORUM_TOPICS T On #F_TOPIC_ID.TOPIC_ID=T.TOPIC_ID);

                DROP TABLE #F_TOPIC_ID;

                CREATE  TABLE #T_TOPICS (
                    COUNT_ID int,
                    TOPICS int,
                    A_TOPICS int,
                    POSTS int,
                    A_POSTS int
                );

                INSERT INTO #T_TOPICS
                    SELECT 1, SUM(F_TOPICS), SUM(F_A_TOPICS),SUM(F_COUNT),SUM(F_A_COUNT) FROM FORUM_FORUM WHERE F_TYPE<>1;

                UPDATE FORUM_TOTALS SET T_COUNT = TOPICS, T_A_COUNT = A_TOPICS, P_COUNT=POSTS, P_A_COUNT=A_POSTS
                    FROM FORUM_TOTALS FT INNER JOIN #T_TOPICS TT ON FT.COUNT_ID=TT.COUNT_ID;

                DROP TABLE #T_TOPICS;

                CREATE TABLE #T_MEMBERS (
                    COUNT_ID int,
                    MEMBERS int
                );

                INSERT INTO #T_MEMBERS
                    SELECT 1, COUNT(MEMBER_ID) FROM FORUM_MEMBERS;

                UPDATE FORUM_TOTALS SET U_COUNT = MEMBERS FROM FORUM_TOTALS FT INNER JOIN #T_MEMBERS TM ON FT.COUNT_ID=TM.COUNT_ID;

                DROP TABLE #T_MEMBERS;

                End

                GO



                ");

            }

            //Console.WriteLine("Initialalize");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE snitz_updateCounts");

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
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
