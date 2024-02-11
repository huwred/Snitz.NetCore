using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class PhotoAlbum2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_FORUM_IMAGES_FORUM_MEMBERS_I_MID",
            //    table: "FORUM_IMAGES");

            //migrationBuilder.DropColumn(
            //    name: "Discriminator",
            //    table: "FORUM_MEMBERS");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "FORUM_MEMBERS",
                keyColumn: "MEMBER_ID",
                keyValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "FORUM_MEMBERS",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "8e445865-a24d-4543-a6c6-9443d048cdb9",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d8148cbd-aca1-49e4-9d31-ee9208df04a4", "AQAAAAIAAYagAAAAEMbK4L6CufMdt08mJC/XixSDngF9Fma698xEx2EXKraCb5mfTGijZURx1Pi0llkwng==", "bb8f3c76-8229-4a78-9f67-c00bcb081ee1" });

            migrationBuilder.InsertData(
                table: "FORUM_MEMBERS",
                columns: new[] { "MEMBER_ID", "M_AGE", "M_AIM", "M_ALLOWEMAIL", "M_BIO", "M_CITY", "M_COUNTRY", "M_DATE", "M_DEFAULT_VIEW", "Discriminator", "M_DOB", "M_EMAIL", "M_FIRSTNAME", "M_HIDE_EMAIL", "M_HOBBIES", "M_HOMEPAGE", "M_ICQ", "M_IP", "M_KEY", "M_LAST_IP", "M_LASTACTIVITY", "M_LASTHEREDATE", "M_LASTNAME", "M_LASTPOSTDATE", "M_LEVEL", "M_LINK1", "M_LINK2", "M_LNEWS", "M_MARSTATUS", "M_MSN", "M_NAME", "M_NEWEMAIL", "M_OCCUPATION", "M_PHOTO_URL", "M_PMEMAIL", "M_PMRECEIVE", "M_PMSAVESENT", "M_POSTS", "M_PRIVATEPROFILE", "M_PWKEY", "M_QUOTE", "M_RECEIVE_EMAIL", "M_SEX", "M_SHA256", "M_SIG_DEFAULT", "M_SIG", "M_STATE", "M_STATUS", "M_SUBSCRIPTION", "M_TITLE", "M_VIEW_SIG", "M_YAHOO" },
                values: new object[] { 1, null, null, (short)0, null, null, null, "20240207143030", 0, "Member", null, "xxxx@example.com", null, (short)0, null, null, null, null, null, null, null, null, null, null, (short)3, null, null, null, null, null, "Administrator", null, null, null, 0, 0, (short)0, 0, (short)0, null, null, (short)0, null, (short)0, (short)0, null, null, (short)1, (short)0, null, (short)0, null });

            migrationBuilder.AddForeignKey(
                name: "FK_FORUM_IMAGES_FORUM_MEMBERS_I_MID",
                table: "FORUM_IMAGES",
                column: "I_MID",
                principalTable: "FORUM_MEMBERS",
                principalColumn: "MEMBER_ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
