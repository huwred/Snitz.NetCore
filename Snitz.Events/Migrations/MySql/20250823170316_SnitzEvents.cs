using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using SnitzCore.Data.Extensions;

#nullable disable

namespace Snitz.Events.Migrations
{
    /// <inheritdoc />
    public partial class SnitzEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            if (!migrationBuilder.ColumnExists("FORUM_FORUM", "F_ALLOWEVENTS"))
            {
                migrationBuilder.AddColumn<int>(
                    name: "F_ALLOWEVENTS",
                    table: "FORUM_FORUM",
                    type: "int",
                    nullable: true);
            }
            if(!migrationBuilder.TableExists("CAL_EVENTS"))
            {
            migrationBuilder.CreateTable(
                name: "CAL_EVENTS",
                columns: table => new
                {
                    C_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TOPIC_ID = table.Column<int>(type: "int", nullable: false),
                    EVENT_ALLDAY = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EVENT_DATE = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EVENT_ENDDATE = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EVENT_RECURS = table.Column<int>(type: "int", nullable: false),
                    EVENT_DAYS = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EVENT_TITLE = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EVENT_DETAILS = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DATE_ADDED = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CLUB_ID = table.Column<int>(type: "int", nullable: true),
                    CAT_ID = table.Column<int>(type: "int", nullable: true),
                    LOC_ID = table.Column<int>(type: "int", nullable: true),
                    AUTHOR_ID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CAL_EVENTS", x => x.C_ID);

                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EVENT_CAT",
                columns: table => new
                {
                    CAT_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CAT_NAME = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CAT_ORDER = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EVENT_CAT", x => x.CAT_ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EVENT_CLUB",
                columns: table => new
                {
                    CLUB_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CLUB_L_NAME = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CLUB_S_NAME = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CLUB_ABBR = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CLUB_ORDER = table.Column<int>(type: "int", nullable: false),
                    CLUB_DEF_LOC = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EVENT_CLUB", x => x.CLUB_ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EVENT_LOCATION",
                columns: table => new
                {
                    LOC_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LOC_NAME = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LOC_ORDER = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EVENT_LOCATION", x => x.LOC_ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

                migrationBuilder.CreateTable(
                    name: "EVENT_SUBSCRIPTIONS",
                    columns: table => new
                    {
                        SUB_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                        CLUB_ID = table.Column<int>(type: "int", nullable: false),
                        MEMBER_ID = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_EVENT_SUBSCRIPTIONS", x => x.SUB_ID);
                    }).Annotation("MySql:CharSet", "utf8mb4");
            }
            migrationBuilder.AddForeignKey(
                name: "FK_CAL_EVENTS_FORUM_TOPICS_TOPIC_ID",
                table: "CAL_EVENTS",
                column: "TOPIC_ID",
                principalTable: "FORUM_TOPICS",
                principalColumn: "TOPIC_ID",
                onDelete: ReferentialAction.Cascade);

                    migrationBuilder.AddForeignKey(
                        name: "FK_CAL_EVENTS_EVENT_CAT_CAT_ID",
                        table: "CAL_EVENTS",
                        column: "CAT_ID",
                        principalTable: "EVENT_CAT",
                        principalColumn: "CAT_ID");

                    migrationBuilder.AddForeignKey(
                        name: "FK_CAL_EVENTS_EVENT_CLUB_CLUB_ID",
                        table: "CAL_EVENTS",
                        column: "CLUB_ID",
                        principalTable: "EVENT_CLUB",
                        principalColumn: "CLUB_ID");
                    migrationBuilder.AddForeignKey(
                        name: "FK_CAL_EVENTS_EVENT_LOCATION_LOC_ID",
                        table: "CAL_EVENTS",
                        column: "LOC_ID",
                        principalTable: "EVENT_LOCATION",
                        principalColumn: "LOC_ID");
                    migrationBuilder.AddForeignKey(
                        name: "FK_CAL_EVENTS_FORUM_MEMBERS_AUTHOR_ID",
                        table: "CAL_EVENTS",
                        column: "AUTHOR_ID",
                        principalTable: "FORUM_MEMBERS",
                        principalColumn: "MEMBER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CAL_EVENTS_AUTHOR_ID",
                table: "CAL_EVENTS",
                column: "AUTHOR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CAL_EVENTS_C_ID",
                table: "CAL_EVENTS",
                column: "C_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CAL_EVENTS_CAT_ID",
                table: "CAL_EVENTS",
                column: "CAT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CAL_EVENTS_CLUB_ID",
                table: "CAL_EVENTS",
                column: "CLUB_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CAL_EVENTS_LOC_ID",
                table: "CAL_EVENTS",
                column: "LOC_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CAL_EVENTS_TOPIC_ID",
                table: "CAL_EVENTS",
                column: "TOPIC_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EVENT_CAT_CAT_ID",
                table: "EVENT_CAT",
                column: "CAT_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EVENT_CLUB_CLUB_ID",
                table: "EVENT_CLUB",
                column: "CLUB_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EVENT_LOCATION_LOC_ID",
                table: "EVENT_LOCATION",
                column: "LOC_ID",
                unique: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropTable(
                name: "CAL_EVENTS");

            migrationBuilder.DropTable(
                name: "EVENT_CAT");

            migrationBuilder.DropTable(
                name: "EVENT_CLUB");

            migrationBuilder.DropTable(
                name: "EVENT_LOCATION");

            migrationBuilder.DropTable(
                name: "EVENT_SUBSCRIPTIONS");


        }
    }
}
