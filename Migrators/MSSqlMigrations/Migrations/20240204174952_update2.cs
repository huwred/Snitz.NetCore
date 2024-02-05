using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class update2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_FORUM_BOOKMARKS_FORUM_FORUM_ForumId",
            //    table: "FORUM_BOOKMARKS");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_FORUM_BOOKMARKS_FORUM_MEMBERS_AuthorId",
            //    table: "FORUM_BOOKMARKS");

            //migrationBuilder.DropIndex(
            //    name: "IX_FORUM_BOOKMARKS_AuthorId",
            //    table: "FORUM_BOOKMARKS");

            //migrationBuilder.DropIndex(
            //    name: "IX_FORUM_BOOKMARKS_ForumId",
            //    table: "FORUM_BOOKMARKS");

            //migrationBuilder.DropColumn(
            //    name: "AuthorId",
            //    table: "FORUM_BOOKMARKS");

            //migrationBuilder.DropColumn(
            //    name: "ForumId",
            //    table: "FORUM_BOOKMARKS");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_BOOKMARKS_B_MEMBERID",
                table: "FORUM_BOOKMARKS",
                column: "B_MEMBERID");

            migrationBuilder.AddForeignKey(
                name: "FK_FORUM_BOOKMARKS_FORUM_MEMBERS_B_MEMBERID",
                table: "FORUM_BOOKMARKS",
                column: "B_MEMBERID",
                principalTable: "FORUM_MEMBERS",
                principalColumn: "MEMBER_ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FORUM_BOOKMARKS_FORUM_MEMBERS_B_MEMBERID",
                table: "FORUM_BOOKMARKS");

            migrationBuilder.DropIndex(
                name: "IX_FORUM_BOOKMARKS_B_MEMBERID",
                table: "FORUM_BOOKMARKS");

            migrationBuilder.AddColumn<int>(
                name: "AuthorId",
                table: "FORUM_BOOKMARKS",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ForumId",
                table: "FORUM_BOOKMARKS",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "8e445865-a24d-4543-a6c6-9443d048cdb9",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "dbd178b8-7fc3-402c-ab0f-b1af3cbca3cd", "AQAAAAIAAYagAAAAEGkp7YRMaOOWDAe12iw3LQk1/zpDkNeSn4iSIzJwUX741OHER6iRm3TTWssiNx2Blw==", "1eea0fc7-7095-4a15-b1ae-a5271c2f732b" });

            migrationBuilder.UpdateData(
                table: "FORUM_MEMBERS",
                keyColumn: "MEMBER_ID",
                keyValue: 1,
                column: "M_DATE",
                value: "20240204174340");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_BOOKMARKS_AuthorId",
                table: "FORUM_BOOKMARKS",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_BOOKMARKS_ForumId",
                table: "FORUM_BOOKMARKS",
                column: "ForumId");

            migrationBuilder.AddForeignKey(
                name: "FK_FORUM_BOOKMARKS_FORUM_FORUM_ForumId",
                table: "FORUM_BOOKMARKS",
                column: "ForumId",
                principalTable: "FORUM_FORUM",
                principalColumn: "FORUM_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_FORUM_BOOKMARKS_FORUM_MEMBERS_AuthorId",
                table: "FORUM_BOOKMARKS",
                column: "AuthorId",
                principalTable: "FORUM_MEMBERS",
                principalColumn: "MEMBER_ID");
        }
    }
}
