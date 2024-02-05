using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class BookMarks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "FORUM_BOOKMARKS",
                columns: table => new
                {
                    BOOKMARK_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    B_TOPICID = table.Column<int>(type: "int", nullable: false),
                    B_MEMBERID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_BOOKMARKS", x => x.BOOKMARK_ID);

                    table.ForeignKey(
                        name: "FK_FORUM_BOOKMARKS_FORUM_TOPICS_B_TOPICID",
                        column: x => x.B_TOPICID,
                        principalTable: "FORUM_TOPICS",
                        principalColumn: "TOPIC_ID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_BOOKMARKS_B_TOPICID",
                table: "FORUM_BOOKMARKS",
                column: "B_TOPICID");


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.DropTable(
                name: "FORUM_BOOKMARKS");

        }
    }
}
