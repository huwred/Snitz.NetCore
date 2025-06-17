using Microsoft.EntityFrameworkCore.Migrations;
using SnitzCore.Data.Extensions;
#nullable disable

namespace Snitz.PostThanks.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostThanks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.TableExists("FORUM_THANKS"))
            {
                return;
            }
            migrationBuilder.CreateTable(
                name: "FORUM_THANKS",
                columns: table => new
                {
                    TOPIC_ID = table.Column<int>(type: "int", nullable: false),
                    MEMBER_ID = table.Column<int>(type: "int", nullable: false),
                    REPLY_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_FORUM_THANKS_FORUM_MEMBERS_MEMBER_ID",
                        column: x => x.MEMBER_ID,
                        principalTable: "FORUM_MEMBERS",
                        principalColumn: "MEMBER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_THANKS_MEMBER_ID",
                table: "FORUM_THANKS",
                column: "MEMBER_ID");

            if (!migrationBuilder.ColumnExists("FORUM_FORUM", "F_ALLOWTHANKS"))
            {
                migrationBuilder.AddColumn<int>(
                    name: "F_ALLOWTHANKS",
                    table: "FORUM_FORUM",
                    type: "int",
                    nullable: true);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FORUM_THANKS");

        }
    }
}
