using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snitz.Events.Migrations
{
    /// <inheritdoc />
    public partial class newmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EVENT_CAT",
                columns: table => new
                {
                    CAT_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CAT_NAME = table.Column<string>(type: "TEXT", nullable: false),
                    CAT_ORDER = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EVENT_CAT", x => x.CAT_ID);
                });

            migrationBuilder.CreateTable(
                name: "EVENT_CLUB",
                columns: table => new
                {
                    CLUB_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CLUB_L_NAME = table.Column<string>(type: "TEXT", nullable: false),
                    CLUB_S_NAME = table.Column<string>(type: "TEXT", nullable: false),
                    CLUB_ABBR = table.Column<string>(type: "TEXT", nullable: false),
                    CLUB_ORDER = table.Column<int>(type: "INTEGER", nullable: false),
                    CLUB_DEF_LOC = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EVENT_CLUB", x => x.CLUB_ID);
                });

            migrationBuilder.CreateTable(
                name: "EVENT_LOCATION",
                columns: table => new
                {
                    LOC_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LOC_NAME = table.Column<string>(type: "TEXT", nullable: false),
                    LOC_ORDER = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EVENT_LOCATION", x => x.LOC_ID);
                });

            migrationBuilder.CreateTable(
                name: "EVENT_SUBSCRIPTIONS",
                columns: table => new
                {
                    SUB_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CLUB_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    MEMBER_ID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EVENT_SUBSCRIPTIONS", x => x.SUB_ID);
                });

            migrationBuilder.CreateTable(
                name: "CAL_EVENTS",
                columns: table => new
                {
                    C_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TOPIC_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    EVENT_ALLDAY = table.Column<bool>(type: "INTEGER", nullable: false),
                    EVENT_DATE = table.Column<string>(type: "TEXT", nullable: true),
                    EVENT_ENDDATE = table.Column<string>(type: "TEXT", nullable: true),
                    EVENT_RECURS = table.Column<int>(type: "INTEGER", nullable: false),
                    EVENT_DAYS = table.Column<string>(type: "TEXT", nullable: true),
                    EVENT_TITLE = table.Column<string>(type: "TEXT", nullable: true),
                    EVENT_DETAILS = table.Column<string>(type: "TEXT", nullable: true),
                    DATE_ADDED = table.Column<string>(type: "TEXT", nullable: true),
                    CLUB_ID = table.Column<int>(type: "INTEGER", nullable: true),
                    CAT_ID = table.Column<int>(type: "INTEGER", nullable: true),
                    LOC_ID = table.Column<int>(type: "INTEGER", nullable: true),
                    AUTHOR_ID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CAL_EVENTS", x => x.C_ID);
                    table.ForeignKey(
                        name: "FK_CAL_EVENTS_EVENT_CAT_CAT_ID",
                        column: x => x.CAT_ID,
                        principalTable: "EVENT_CAT",
                        principalColumn: "CAT_ID");
                    table.ForeignKey(
                        name: "FK_CAL_EVENTS_EVENT_CLUB_CLUB_ID",
                        column: x => x.CLUB_ID,
                        principalTable: "EVENT_CLUB",
                        principalColumn: "CLUB_ID");
                    table.ForeignKey(
                        name: "FK_CAL_EVENTS_EVENT_LOCATION_LOC_ID",
                        column: x => x.LOC_ID,
                        principalTable: "EVENT_LOCATION",
                        principalColumn: "LOC_ID");
                    table.ForeignKey(
                        name: "FK_CAL_EVENTS_FORUM_MEMBERS_AUTHOR_ID",
                        column: x => x.AUTHOR_ID,
                        principalTable: "FORUM_MEMBERS",
                        principalColumn: "MEMBER_ID");
                    table.ForeignKey(
                        name: "FK_CAL_EVENTS_FORUM_TOPICS_TOPIC_ID",
                        column: x => x.TOPIC_ID,
                        principalTable: "FORUM_TOPICS",
                        principalColumn: "TOPIC_ID",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.AddColumn<int>(
                name: "F_ALLOWEVENTS",
                table: $"FORUM_FORUM",
                type: "INTEGER",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CAL_EVENTS_AUTHOR_ID",
                table: "CAL_EVENTS",
                column: "AUTHOR_ID");

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

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CAL_EVENTS");

            migrationBuilder.DropTable(
                name: "EVENT_SUBSCRIPTIONS");

            migrationBuilder.DropTable(
                name: "EVENT_CAT");

            migrationBuilder.DropTable(
                name: "EVENT_CLUB");

            migrationBuilder.DropTable(
                name: "EVENT_LOCATION");

        }
    }
}
