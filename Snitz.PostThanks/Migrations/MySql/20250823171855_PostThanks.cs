using Microsoft.EntityFrameworkCore.Metadata;
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
            //if (migrationBuilder.TableExists("FORUM_THANKS"))
            //{
            //    return;
            //}
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

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
                }).Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_THANKS_MEMBER_ID",
                table: "FORUM_THANKS",
                column: "MEMBER_ID");

                migrationBuilder.AddColumn<int>(
                    name: "F_ALLOWTHANKS",
                    table: "FORUM_FORUM",
                    type: "int",
                    defaultValue: 0,
                    nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FORUM_THANKS");

        }
    }
}
