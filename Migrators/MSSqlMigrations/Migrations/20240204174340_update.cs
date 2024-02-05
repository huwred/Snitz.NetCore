using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_BOOKMARKS_AuthorId",
                table: "FORUM_BOOKMARKS",
                column: "B_MEMBERID");

            migrationBuilder.AddForeignKey(
                name: "FK_FORUM_BOOKMARKS_FORUM_MEMBERS_AuthorId",
                table: "FORUM_BOOKMARKS",
                column: "B_MEMBERID",
                principalTable: "FORUM_MEMBERS",
                principalColumn: "MEMBER_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropForeignKey(
                name: "FK_FORUM_BOOKMARKS_FORUM_MEMBERS_AuthorId",
                table: "FORUM_BOOKMARKS");

            migrationBuilder.DropIndex(
                name: "IX_FORUM_BOOKMARKS_AuthorId",
                table: "FORUM_BOOKMARKS");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_BOOKMARKS_B_MEMBERID",
                table: "FORUM_BOOKMARKS",
                column: "B_MEMBERID");

        }
    }
}
