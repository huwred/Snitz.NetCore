using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snitz.Events.Migrations
{
    /// <inheritdoc />
    public partial class SnitzEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "CAL_EVENTS",
                columns: table => new
                {
                    C_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TOPIC_ID = table.Column<int>(type: "int", nullable: false),
                    EVENT_ALLDAY = table.Column<bool>(type: "bit", nullable: false),
                    EVENT_DATE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EVENT_ENDDATE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EVENT_RECURS = table.Column<int>(type: "int", nullable: false),
                    EVENT_DAYS = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Author = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CAL_EVENTS", x => x.C_ID);
                });

            migrationBuilder.CreateTable(
                name: "EVENT_CAT",
                columns: table => new
                {
                    CAT_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CAT_NAME = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CAT_ORDER = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EVENT_CAT", x => x.CAT_ID);
                });

            migrationBuilder.CreateTable(
                name: "EVENT_CLUB",
                columns: table => new
                {
                    CLUB_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CLUB_L_NAME = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CLUB_S_NAME = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CLUB_ABBR = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CLUB_ORDER = table.Column<int>(type: "int", nullable: false),
                    CLUB_DEF_LOC = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EVENT_CLUB", x => x.CLUB_ID);
                });

            migrationBuilder.CreateTable(
                name: "EVENT_LOCATION",
                columns: table => new
                {
                    LOC_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LOC_NAME = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LOC_ORDER = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EVENT_LOCATION", x => x.LOC_ID);
                });

            migrationBuilder.CreateTable(
                name: "EVENT_SUBSCRIPTIONS",
                columns: table => new
                {
                    SUB_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CLUB_ID = table.Column<int>(type: "int", nullable: false),
                    MEMBER_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EVENT_SUBSCRIPTIONS", x => x.SUB_ID);
                });


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
