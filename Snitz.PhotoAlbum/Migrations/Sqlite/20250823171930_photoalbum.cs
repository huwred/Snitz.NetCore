using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snitz.PhotoAlbum.Migrations
{
    /// <inheritdoc />
    public partial class newmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FORUM_IMAGE_CAT",
                columns: table => new
                {
                    CAT_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MEMBER_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    CAT_DESC = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_IMAGE_CAT", x => x.CAT_ID);
                });
            migrationBuilder.CreateTable(
                    name: "FORUM_ORG_GROUP",
                    columns: table => new
                    {
                        O_GROUP_ID = table.Column<int>(type: "INTEGER", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        O_GROUP_ORDER = table.Column<int>(type: "INTEGER", nullable: false),
                        O_GROUP_NAME = table.Column<string>(type: "TEXT", nullable: true)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_ORG_GROUP", x => x.O_GROUP_ID);
                    });
            migrationBuilder.CreateTable(
                name: "FORUM_IMAGES",
                columns: table => new
                {
                    I_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    I_MID = table.Column<int>(type: "INTEGER", nullable: false),
                    I_CAT = table.Column<int>(type: "INTEGER", nullable: true),
                    I_WIDTH = table.Column<int>(type: "INTEGER", nullable: true),
                    I_HEIGHT = table.Column<int>(type: "INTEGER", nullable: true),
                    I_SIZE = table.Column<int>(type: "INTEGER", nullable: false),
                    I_VIEWS = table.Column<int>(type: "INTEGER", nullable: true),
                    I_LOC = table.Column<string>(type: "TEXT", nullable: true),
                    I_DESC = table.Column<string>(type: "TEXT", nullable: true),
                    I_TYPE = table.Column<string>(type: "TEXT", nullable: true),
                    I_DATE = table.Column<string>(type: "TEXT", nullable: true),
                    I_GROUP_ID = table.Column<int>(type: "INTEGER", nullable: true),
                    I_SCIENTIFICNAME = table.Column<string>(type: "TEXT", nullable: true),
                    I_NORWEGIANNAME = table.Column<string>(type: "TEXT", nullable: true),
                    I_PRIVATE = table.Column<bool>(type: "INTEGER", nullable: false),
                    I_NOTFEATURED = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_IMAGES", x => x.I_ID);
                    table.ForeignKey(
                        name: "FK_FORUM_IMAGES_FORUM_IMAGE_CAT_I_CAT",
                        column: x => x.I_CAT,
                        principalTable: "FORUM_IMAGE_CAT",
                        principalColumn: "CAT_ID");
                    table.ForeignKey(
                        name: "FK_FORUM_IMAGES_FORUM_MEMBERS_I_MID",
                        column: x => x.I_MID,
                        principalTable: "FORUM_MEMBERS",
                        principalColumn: "MEMBER_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FORUM_IMAGES_FORUM_ORG_GROUP_I_GROUP_ID",
                        
                        column: x => x.I_GROUP_ID,
                        principalTable: "FORUM_ORG_GROUP",
                        principalColumn: "O_GROUP_ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_IMAGES_I_CAT",
                table: "FORUM_IMAGES",
                column: "I_CAT");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_IMAGES_I_GROUP_ID",
                table: "FORUM_IMAGES",
                column: "I_GROUP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_IMAGES_I_MID",
                table: "FORUM_IMAGES",
                column: "I_MID");

            migrationBuilder.InsertData(
                table: $"FORUM_IMAGE_CAT",
                columns: new[] { "CAT_ID", "MEMBER_ID", "CAT_DESC" },
                columnTypes: new[] { "INTEGER", "INTEGER", "VARCHAR(255)"},
                values: new object[] { 1, 1, "General"});

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropTable(
                name: "FORUM_IMAGES");

            migrationBuilder.DropTable(
                name: "FORUM_IMAGE_CAT");

        }
    }
}
