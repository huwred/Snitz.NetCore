using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snitz.PhotoAlbum.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FORUM_A_REPLY");

            migrationBuilder.DropTable(
                name: "FORUM_A_TOPICS");
        }
    }
}
