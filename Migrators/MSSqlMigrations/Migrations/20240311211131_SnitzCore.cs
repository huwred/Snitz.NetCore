using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class SnitzCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.AddColumn<bool>(
                name: "T_ANSWERED",
                table: "FORUM_TOPICS",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "R_ANSWER",
                table: "FORUM_REPLY",
                type: "bit",
                nullable: false,
                defaultValue: false);

            //migrationBuilder.AlterColumn<int>(
            //    name: "GROUP_CATID",
            //    table: "FORUM_GROUPS",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0,
            //    oldClrType: typeof(int),
            //    oldType: "int",
            //    oldNullable: true);

            //migrationBuilder.CreateTable(
            //    name: "FORUM_BOOKMARKS",
            //    columns: table => new
            //    {
            //        BOOKMARK_ID = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        B_TOPICID = table.Column<int>(type: "int", nullable: false),
            //        B_MEMBERID = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_FORUM_BOOKMARKS", x => x.BOOKMARK_ID);
            //        table.ForeignKey(
            //            name: "FK_FORUM_BOOKMARKS_FORUM_MEMBERS_B_MEMBERID",
            //            column: x => x.B_MEMBERID,
            //            principalTable: "FORUM_MEMBERS",
            //            principalColumn: "MEMBER_ID",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_FORUM_BOOKMARKS_FORUM_TOPICS_B_TOPICID",
            //            column: x => x.B_TOPICID,
            //            principalTable: "FORUM_TOPICS",
            //            principalColumn: "TOPIC_ID",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "FORUM_IMAGE_CAT",
            //    columns: table => new
            //    {
            //        CAT_ID = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        MEMBER_ID = table.Column<int>(type: "int", nullable: false),
            //        CAT_DESC = table.Column<string>(type: "nvarchar(max)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_FORUM_IMAGE_CAT", x => x.CAT_ID);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "FORUM_ORG_GROUP",
            //    columns: table => new
            //    {
            //        O_GROUP_ID = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        O_GROUP_ORDER = table.Column<int>(type: "int", nullable: false),
            //        O_GROUP_NAME = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_FORUM_ORG_GROUP", x => x.O_GROUP_ID);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "FORUM_IMAGES",
            //    columns: table => new
            //    {
            //        I_ID = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        I_MID = table.Column<int>(type: "int", nullable: false),
            //        I_CAT = table.Column<int>(type: "int", nullable: true),
            //        I_WIDTH = table.Column<int>(type: "int", nullable: true),
            //        I_HEIGHT = table.Column<int>(type: "int", nullable: true),
            //        I_SIZE = table.Column<int>(type: "int", nullable: false),
            //        I_VIEWS = table.Column<int>(type: "int", nullable: true),
            //        I_LOC = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        I_DESC = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        I_TYPE = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        I_DATE = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        I_GROUP_ID = table.Column<int>(type: "int", nullable: true),
            //        I_SCIENTIFICNAME = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        I_NORWEGIANNAME = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        I_PRIVATE = table.Column<bool>(type: "bit", nullable: false),
            //        I_NOTFEATURED = table.Column<bool>(type: "bit", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_FORUM_IMAGES", x => x.I_ID);
            //        table.ForeignKey(
            //            name: "FK_FORUM_IMAGES_FORUM_IMAGE_CAT_I_CAT",
            //            column: x => x.I_CAT,
            //            principalTable: "FORUM_IMAGE_CAT",
            //            principalColumn: "CAT_ID");
            //        table.ForeignKey(
            //            name: "FK_FORUM_IMAGES_FORUM_MEMBERS_I_MID",
            //            column: x => x.I_MID,
            //            principalTable: "FORUM_MEMBERS",
            //            principalColumn: "MEMBER_ID",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_FORUM_IMAGES_FORUM_ORG_GROUP_I_GROUP_ID",
            //            column: x => x.I_GROUP_ID,
            //            principalTable: "FORUM_ORG_GROUP",
            //            principalColumn: "O_GROUP_ID");
            //    });

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_MODERATOR_FORUM_ID",
                table: "FORUM_MODERATOR",
                column: "FORUM_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_MODERATOR_MEMBER_ID",
                table: "FORUM_MODERATOR",
                column: "MEMBER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_GROUPS_GROUP_CATID",
                table: "FORUM_GROUPS",
                column: "GROUP_CATID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_GROUPS_GROUP_ID",
                table: "FORUM_GROUPS",
                column: "GROUP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_BOOKMARKS_B_MEMBERID",
                table: "FORUM_BOOKMARKS",
                column: "B_MEMBERID");

            migrationBuilder.CreateIndex(
                name: "IX_FORUM_BOOKMARKS_B_TOPICID",
                table: "FORUM_BOOKMARKS",
                column: "B_TOPICID");

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

            migrationBuilder.AddForeignKey(
                name: "FK_FORUM_MODERATOR_FORUM_FORUM_FORUM_ID",
                table: "FORUM_MODERATOR",
                column: "FORUM_ID",
                principalTable: "FORUM_FORUM",
                principalColumn: "FORUM_ID",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_FORUM_MODERATOR_FORUM_MEMBERS_MEMBER_ID",
                table: "FORUM_MODERATOR",
                column: "MEMBER_ID",
                principalTable: "FORUM_MEMBERS",
                principalColumn: "MEMBER_ID",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropForeignKey(
                name: "FK_FORUM_MODERATOR_FORUM_FORUM_FORUM_ID",
                table: "FORUM_MODERATOR");

            migrationBuilder.DropForeignKey(
                name: "FK_FORUM_MODERATOR_FORUM_MEMBERS_MEMBER_ID",
                table: "FORUM_MODERATOR");

            migrationBuilder.DropTable(
                name: "FORUM_BOOKMARKS");

            migrationBuilder.DropTable(
                name: "FORUM_IMAGES");

            migrationBuilder.DropTable(
                name: "FORUM_IMAGE_CAT");

            migrationBuilder.DropTable(
                name: "FORUM_ORG_GROUP");

            migrationBuilder.DropIndex(
                name: "IX_FORUM_MODERATOR_FORUM_ID",
                table: "FORUM_MODERATOR");

            migrationBuilder.DropIndex(
                name: "IX_FORUM_MODERATOR_MEMBER_ID",
                table: "FORUM_MODERATOR");

            migrationBuilder.DropIndex(
                name: "IX_FORUM_GROUPS_GROUP_CATID",
                table: "FORUM_GROUPS");

            migrationBuilder.DropIndex(
                name: "IX_FORUM_GROUPS_GROUP_ID",
                table: "FORUM_GROUPS");

            migrationBuilder.DropColumn(
                name: "T_ANSWERED",
                table: "FORUM_TOPICS");

            migrationBuilder.DropColumn(
                name: "R_ANSWER",
                table: "FORUM_REPLY");

            migrationBuilder.AddColumn<int>(
                name: "LastPoster",
                table: "FORUM_TOPICS",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GROUP_CATID",
                table: "FORUM_GROUPS",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "LastPoster",
                table: "FORUM_A_TOPICS",
                type: "int",
                nullable: true);

        }
    }
}
