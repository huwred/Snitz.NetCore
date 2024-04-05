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
                            .Annotation("SqlServer:Identity", "1, 1"),
                        TOPIC_ID = table.Column<int>(type: "int", nullable: false),
                        EVENT_ALLDAY = table.Column<bool>(type: "bit", nullable: false),
                        EVENT_DATE = table.Column<string>(type: "varchar(14)", nullable: true),
                        EVENT_ENDDATE = table.Column<string>(type: "varchar(14)", nullable: true),
                        EVENT_RECURS = table.Column<int>(type: "int", nullable: false),
                        EVENT_DAYS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        EVENT_TITLE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        EVENT_DETAILS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        AUTHOR_ID = table.Column<int>(type: "int", nullable: true),
                        DATE_ADDED = table.Column<string>(type: "varchar(14)", nullable: true),
                        CLUB_ID = table.Column<int>(type: "int", nullable: true),
                        CAT_ID = table.Column<int>(type: "int", nullable: true),
                        LOC_ID = table.Column<int>(type: "int", nullable: true)
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

            if (!migrationBuilder.IndexExists(
                    "SELECT COUNT(*) FROM sys.indexes WHERE object_id = OBJECT_ID('EVENT_LOCATION') AND name='IX_EVENT_LOCATION_LOC_ID'"))
            {
                migrationBuilder.CreateIndex(
                    name: "IX_EVENT_LOCATION_LOC_ID",
                    table: "EVENT_LOCATION",
                    column: "LOC_ID",
                    unique: true);
            }
            if (!migrationBuilder.IndexExists(
                    "SELECT COUNT(*) FROM sys.indexes WHERE object_id = OBJECT_ID('EVENT_CLUB') AND name='IX_EVENT_CLUB_CLUB_ID'"))
            {
                migrationBuilder.CreateIndex(
                    name: "IX_EVENT_CLUB_CLUB_ID",
                    table: "EVENT_CLUB",
                    column: "CLUB_ID",
                    unique: true);
            }
            if (!migrationBuilder.IndexExists(
                    "SELECT COUNT(*) FROM sys.indexes WHERE object_id = OBJECT_ID('EVENT_CAT') AND name='IX_EVENT_CAT_CAT_ID'"))
            {
                migrationBuilder.CreateIndex(
                    name: "IX_EVENT_CAT_CAT_ID",
                    table: "EVENT_CAT",
                    column: "CAT_ID",
                    unique: true);
            }



            if (!migrationBuilder.IndexExists(
                    "SELECT COUNT(*) FROM sys.indexes WHERE object_id = OBJECT_ID('CAL_EVENTS') AND name='IX_CAL_EVENTS_C_ID'"))
            {
                migrationBuilder.CreateIndex(
                    name: "IX_CAL_EVENTS_C_ID",
                    table: "CAL_EVENTS",
                    column: "C_ID",
                    unique: true);
            }

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
