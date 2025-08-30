using Microsoft.EntityFrameworkCore.Migrations;
using SnitzCore.Data.Models;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class BookMarks : SnitzMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            SetParameters();
            migrationBuilder.CreateTable(
                name: $"{_forumTablePrefix}BOOKMARKS",
                columns: table => new
                {
                    BOOKMARK_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    B_TOPICID = table.Column<int>(type: "INTEGER", nullable: false),
                    B_MEMBERID = table.Column<int>(type: "INTEGER", nullable: false),
                    AuthorId = table.Column<int>(type: "INTEGER", nullable: false),
                    ForumId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_BOOKMARKS", x => x.BOOKMARK_ID);
                    table.ForeignKey(
                        name: "FK_FORUM_BOOKMARKS_FORUM_FORUM_ForumId",
                        column: x => x.ForumId,
                        principalTable: $"{_forumTablePrefix}FORUM",
                        principalColumn: "FORUM_ID",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_FORUM_BOOKMARKS_FORUM_MEMBERS_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: $"{_memberTablePrefix}MEMBERS",
                        principalColumn: "MEMBER_ID",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_FORUM_BOOKMARKS_FORUM_TOPICS_B_TOPICID",
                        column: x => x.B_TOPICID,
                        principalTable: $"{_forumTablePrefix}TOPICS",
                        principalColumn: "TOPIC_ID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_BOOKMARKS_AuthorId",
                table: $"{_forumTablePrefix}BOOKMARKS",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_BOOKMARKS_B_TOPICID",
                table: $"{_forumTablePrefix}BOOKMARKS",
                column: "B_TOPICID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_BOOKMARKS_ForumId",
                table: $"{_forumTablePrefix}BOOKMARKS",
                column: "ForumId");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropTable(
                name: "FORUM_BOOKMARKS");

        }
    }
}
