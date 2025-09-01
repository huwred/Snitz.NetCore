using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using SnitzCore.Data.Extensions;
using System;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Snitz.PhotoAlbum.Migrations
{
    /// <inheritdoc />
    public partial class PhotoAlbum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.TableExists("FORUM_IMAGES"))
            {
                return;
            }
            migrationBuilder.CreateTable(
                name: "FORUM_IMAGE_CAT",
                columns: table => new
                {
                    CAT_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MEMBER_ID = table.Column<int>(type: "int", nullable: false),
                    CAT_DESC = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_IMAGE_CAT", x => x.CAT_ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FORUM_IMAGES",
                columns: table => new
                {
                    I_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    I_MID = table.Column<int>(type: "int", nullable: false),
                    I_CAT = table.Column<int>(type: "int", nullable: true),
                    I_WIDTH = table.Column<int>(type: "int", nullable: true),
                    I_HEIGHT = table.Column<int>(type: "int", nullable: true),
                    I_SIZE = table.Column<int>(type: "int", nullable: false),
                    I_VIEWS = table.Column<int>(type: "int", nullable: true),
                    I_LOC = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    I_DESC = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    I_TYPE = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    I_DATE = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    I_GROUP_ID = table.Column<int>(type: "int", nullable: true),
                    I_SCIENTIFICNAME = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    I_NORWEGIANNAME = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    I_PRIVATE = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    I_NOTFEATURED = table.Column<bool>(type: "tinyint(1)", nullable: false)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_IMAGES_I_CAT",
                table: "FORUM_IMAGES",
                column: "I_CAT");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_IMAGES_I_GROUP_ID",
                table: "FORUM_IMAGES",
                column: "I_GROUP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_IMAGES_I_ID",
                table: "FORUM_IMAGES",
                column: "I_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_IMAGES_I_MID",
                table: "FORUM_IMAGES",
                column: "I_MID");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FORUM_IMAGES");

            migrationBuilder.DropTable(
                name: "FORUM_IMAGE_CAT");

            migrationBuilder.DropTable(
                name: "FORUM_ORG_GROUP");

        }
    }
}
