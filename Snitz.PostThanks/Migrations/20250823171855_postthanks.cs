using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snitz.PostThanks.Migrations
{
    /// <inheritdoc />
    public partial class newmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "FORUM_THANKS",
                columns: table => new
                {
                    TOPIC_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    MEMBER_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    REPLY_ID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_THANKS", x => new { x.MEMBER_ID, x.TOPIC_ID, x.REPLY_ID });
                    table.ForeignKey(
                        name: "FK_FORUM_THANKS_FORUM_MEMBERS_MEMBER_ID",
                        column: x => x.MEMBER_ID,
                        principalTable: "FORUM_MEMBERS",
                        principalColumn: "MEMBER_ID",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.AddColumn<int>(
                name: "F_ALLOWTHANKS",
                table: $"FORUM_FORUM",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropTable(
                name: "FORUM_THANKS");

        }
    }
}
