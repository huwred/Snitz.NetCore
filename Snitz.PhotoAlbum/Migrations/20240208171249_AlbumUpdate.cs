using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snitz.PhotoAlbum.Migrations
{
    /// <inheritdoc />
    public partial class AlbumUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FORUM_IMAGES_FORUM_ORG_GROUP_I_GROUP_ID",
                table: "FORUM_IMAGES");

            migrationBuilder.AlterColumn<int>(
                name: "I_WIDTH",
                table: "FORUM_IMAGES",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "I_VIEWS",
                table: "FORUM_IMAGES",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "I_TYPE",
                table: "FORUM_IMAGES",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "I_SCIENTIFICNAME",
                table: "FORUM_IMAGES",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "I_NORWEGIANNAME",
                table: "FORUM_IMAGES",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "I_HEIGHT",
                table: "FORUM_IMAGES",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "I_GROUP_ID",
                table: "FORUM_IMAGES",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_FORUM_IMAGES_FORUM_MEMBERS_I_MID",
                table: "FORUM_IMAGES",
                column: "I_MID",
                principalTable: "FORUM_MEMBERS",
                principalColumn: "MEMBER_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FORUM_IMAGES_FORUM_ORG_GROUP_I_GROUP_ID",
                table: "FORUM_IMAGES",
                column: "I_GROUP_ID",
                principalTable: "FORUM_ORG_GROUP",
                principalColumn: "O_GROUP_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FORUM_IMAGES_FORUM_MEMBERS_I_MID",
                table: "FORUM_IMAGES");

            migrationBuilder.DropForeignKey(
                name: "FK_FORUM_IMAGES_FORUM_ORG_GROUP_I_GROUP_ID",
                table: "FORUM_IMAGES");

            migrationBuilder.AlterColumn<int>(
                name: "I_WIDTH",
                table: "FORUM_IMAGES",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "I_VIEWS",
                table: "FORUM_IMAGES",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "I_TYPE",
                table: "FORUM_IMAGES",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "I_SCIENTIFICNAME",
                table: "FORUM_IMAGES",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "I_NORWEGIANNAME",
                table: "FORUM_IMAGES",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "I_HEIGHT",
                table: "FORUM_IMAGES",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "I_GROUP_ID",
                table: "FORUM_IMAGES",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "8e445865-a24d-4543-a6c6-9443d048cdb9",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "1415e848-8f31-42f4-9b84-665d7bd1c712", "AQAAAAIAAYagAAAAELd5w2jI73vpOKmGRQRFcoleuhP+gOHKS0xKy9H4eNLfXBBAfwE+FejGklQcxx4X3g==", "7f3404fb-7e15-4717-85de-7f0da42b96f8" });

            migrationBuilder.UpdateData(
                table: "FORUM_MEMBERS",
                keyColumn: "MEMBER_ID",
                keyValue: 1,
                column: "M_DATE",
                value: "20240207144520");

            migrationBuilder.AddForeignKey(
                name: "FK_FORUM_IMAGES_FORUM_ORG_GROUP_I_GROUP_ID",
                table: "FORUM_IMAGES",
                column: "I_GROUP_ID",
                principalTable: "FORUM_ORG_GROUP",
                principalColumn: "O_GROUP_ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
