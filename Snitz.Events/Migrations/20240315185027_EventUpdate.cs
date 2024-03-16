using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snitz.Events.Migrations
{
    /// <inheritdoc />
    public partial class EventUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "CAL_EVENTS");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "CAL_EVENTS");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "CAL_EVENTS",
                newName: "EVENT_DETAILS");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "CAL_EVENTS",
                newName: "EVENT_TITLE");

            migrationBuilder.RenameColumn(
                name: "Author",
                table: "CAL_EVENTS",
                newName: "DATE_ADDED");

            migrationBuilder.AddColumn<int>(
                name: "AUTHOR_ID",
                table: "CAL_EVENTS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CAT_ID",
                table: "CAL_EVENTS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CLUB_ID",
                table: "CAL_EVENTS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LOC_ID",
                table: "CAL_EVENTS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EVENT_LOCATION_LOC_ID",
                table: "EVENT_LOCATION",
                column: "LOC_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EVENT_CLUB_CLUB_ID",
                table: "EVENT_CLUB",
                column: "CLUB_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EVENT_CAT_CAT_ID",
                table: "EVENT_CAT",
                column: "CAT_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CAL_EVENTS_C_ID",
                table: "CAL_EVENTS",
                column: "C_ID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EVENT_LOCATION_LOC_ID",
                table: "EVENT_LOCATION");

            migrationBuilder.DropIndex(
                name: "IX_EVENT_CLUB_CLUB_ID",
                table: "EVENT_CLUB");

            migrationBuilder.DropIndex(
                name: "IX_EVENT_CAT_CAT_ID",
                table: "EVENT_CAT");

            migrationBuilder.DropIndex(
                name: "IX_CAL_EVENTS_C_ID",
                table: "CAL_EVENTS");

            migrationBuilder.DropColumn(
                name: "AUTHOR_ID",
                table: "CAL_EVENTS");

            migrationBuilder.DropColumn(
                name: "CAT_ID",
                table: "CAL_EVENTS");

            migrationBuilder.DropColumn(
                name: "CLUB_ID",
                table: "CAL_EVENTS");

            migrationBuilder.DropColumn(
                name: "LOC_ID",
                table: "CAL_EVENTS");

            migrationBuilder.RenameColumn(
                name: "EVENT_DETAILS",
                table: "CAL_EVENTS",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "EVENT_TITLE",
                table: "CAL_EVENTS",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "DATE_ADDED",
                table: "CAL_EVENTS",
                newName: "Author");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "CAL_EVENTS",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "CAL_EVENTS",
                type: "datetime2",
                nullable: true);


        }
    }
}
