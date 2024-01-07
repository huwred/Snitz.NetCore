using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using System.Reflection;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        public string _forumTablePrefix;
        public string _memberTablePrefix;
        private void SetParameters()
        {
            var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var builder = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
            var config = builder.Build();

            _forumTablePrefix = config.GetSection("SnitzForums").GetSection("forumTablePrefix").Value;
            _memberTablePrefix = config.GetSection("SnitzForums").GetSection("memberTablePrefix").Value;

        }
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            SetParameters();
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "8e445865-a24d-4543-a6c6-9443d048cdb9",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "5b9d430b-3b6d-49e1-88ba-058fb91ea5f0", "AQAAAAIAAYagAAAAEDpJ7OCmCWQoo5vWLBKEQd7FmcPMQT2GiDY+68IcM0ybn+Up3tsoEK6y6oQiY2DUbw==", "9b7f7d0e-fed1-44c2-bb3b-ebe4b34bf17f" });

            migrationBuilder.InsertData(
                table: $"{_forumTablePrefix}CONFIG_NEW",
                columns: new[] { "ID", "C_VARIABLE", "C_VALUE" },
                values: new object[,]
                {
                    { 1, "STRICONS", "1" },
                    { 2, "STRALLOWFORUMCODE", "1" },
                    { 3, "STRPHOTOALBUM", "1" },
                    { 4, "STRBADWORDFILTER", "1" },
                    { 5, "STRIMGINPOSTS", "1" },
                    { 6, "INTHOTTOPICNUM", "25" },
                    { 7, "STRPAGESIZE", "15" },
                    { 8, "STRPAGENUMBERSIZE", "10" },
                    { 9, "STRMARSTATUS", "1" },
                    { 10, "STRFULLNAME", "1" },
                    { 11, "STRPICTURE", "1" },
                    { 12, "STRSEX", "1" },
                    { 13, "STRCITY", "1" },
                    { 14, "STRSTATE", "1" },
                    { 15, "STRAGE", "0" },
                    { 16, "STRAGEDOB", "1" },
                    { 17, "STRMINAGE", "14" },
                    { 18, "STRCOUNTRY", "1" },
                    { 19, "STROCCUPATION", "1" },
                    { 20, "STRFAVLINKS", "1" },
                    { 21, "STRBIO", "1" },
                    { 22, "STRHOBBIES", "1" },
                    { 23, "STRLNEWS", "1" },
                    { 24, "STRQUOTE", "1" },
                    { 25, "STRHOMEPAGE", "1" }
                });

            migrationBuilder.InsertData(
                table: $"{_forumTablePrefix}RANKING",
                columns: new[] { "RANK_ID", "R_IMAGE", "R_IMG_REPEAT", "R_POSTS", "R_TITLE" },
                values: new object[,]
                {
                    { 0, "gold", 5, 0, "Administrator" },
                    { 1, "silver", 5, 0, "Moderator" },
                    { 2, "bronze", 0, 0, "Starting Member" },
                    { 3, "green", 1, 5, "Newbie" },
                    { 4, "cyan", 2, 50, "Junior" },
                    { 5, "blue", 3, 250, "Average Member" },
                    { 6, "purple", 4, 500, "Knowitall" },
                    { 7, "bronze", 5, 2000, "Forum Guru" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}CONFIG_NEW",
                keyColumn: "ID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}CONFIG_NEW",
                keyColumn: "ID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}CONFIG_NEW",
                keyColumn: "ID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}CONFIG_NEW",
                keyColumn: "ID",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}CONFIG_NEW",
                keyColumn: "ID",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}CONFIG_NEW",
                keyColumn: "ID",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}CONFIG_NEW",
                keyColumn: "ID",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}CONFIG_NEW",
                keyColumn: "ID",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}CONFIG_NEW",
                keyColumn: "ID",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}CONFIG_NEW",
                keyColumn: "ID",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}CONFIG_NEW",
                keyColumn: "ID",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}CONFIG_NEW",
                keyColumn: "ID",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}CONFIG_NEW",
                keyColumn: "ID",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}CONFIG_NEW",
                keyColumn: "ID",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}CONFIG_NEW",
                keyColumn: "ID",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}CONFIG_NEW",
                keyColumn: "ID",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}CONFIG_NEW",
                keyColumn: "ID",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}CONFIG_NEW",
                keyColumn: "ID",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}CONFIG_NEW",
                keyColumn: "ID",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}CONFIG_NEW",
                keyColumn: "ID",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}CONFIG_NEW",
                keyColumn: "ID",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}CONFIG_NEW",
                keyColumn: "ID",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}CONFIG_NEW",
                keyColumn: "ID",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}CONFIG_NEW",
                keyColumn: "ID",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}CONFIG_NEW",
                keyColumn: "ID",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}RANKING",
                keyColumn: "RANK_ID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}RANKING",
                keyColumn: "RANK_ID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}RANKING",
                keyColumn: "RANK_ID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}RANKING",
                keyColumn: "RANK_ID",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}RANKING",
                keyColumn: "RANK_ID",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}RANKING",
                keyColumn: "RANK_ID",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}RANKING",
                keyColumn: "RANK_ID",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: $"{_forumTablePrefix}RANKING",
                keyColumn: "RANK_ID",
                keyValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "8e445865-a24d-4543-a6c6-9443d048cdb9",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "29c97079-9e3f-4e2c-a92d-748fbc19cadd", "AQAAAAIAAYagAAAAEAKKzb1/o7lXOxswnPwUc2D7fveBPHAti1fcyyWFjHUh49Tga2IpfkuAB7BTaiYmjg==", "6efdd0a9-6c20-44ea-8a4d-dec3ebd2240b" });


        }
    }
}
