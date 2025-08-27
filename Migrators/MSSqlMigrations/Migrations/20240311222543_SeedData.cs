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
                    { 25, "STRHOMEPAGE", "1" },
                    { 25, "INTMAXFILESIZE", "5" }
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

            migrationBuilder.InsertData(
                table: $"{_memberTablePrefix}MEMBERS",
                columns: new[] { "MEMBER_ID", "M_AGE", "M_AIM", "M_ALLOWEMAIL", "M_BIO", "M_CITY", "M_COUNTRY", "M_DATE", "M_DEFAULT_VIEW", "M_DOB", "M_EMAIL", "M_FIRSTNAME", "M_HIDE_EMAIL", "M_HOBBIES", "M_HOMEPAGE", "M_ICQ", "M_IP", "M_KEY", "M_LAST_IP", "M_LastLogin", "M_LASTNAME", "M_LASTPOSTDATE", "M_LEVEL", "M_LINK1", "M_LINK2", "M_LNEWS", "M_MARSTATUS", "M_MSN", "M_NAME", "M_NEWEMAIL", "M_OCCUPATION", "M_PHOTO_URL", "M_POSTS", "M_PWKEY", "M_QUOTE", "M_RECEIVE_EMAIL", "M_SEX", "M_SHA256", "M_SIG_DEFAULT", "M_SIG", "M_STATE", "M_STATUS", "M_SUBSCRIPTION", "M_TITLE", "M_VIEW_SIG", "M_YAHOO" },
                values: new object[] { 1, null, null, (short)0, null, null, null, "20231230082941", 0, null, "xxxx@example.com", null, (short)0, null, null, null, null, null, null, null, null, null, (short)3, null, null, null, null, null, "Administrator", null, null, null, 0, null, null, (short)0, null, (short)0, (short)0, null, null, (short)1, (short)0, null, (short)0, null });

            migrationBuilder.InsertData(
                table: $"{_forumTablePrefix}CATEGORY",
                columns: new[] { "CAT_ID", "CAT_MODERATION", "CAT_NAME", "CAT_ORDER", "CAT_STATUS", "CAT_SUBSCRIPTION" },
                values: new object[] { 1, 0, "General", 0, (short)1, 0 });

            migrationBuilder.InsertData(
                table: $"{_forumTablePrefix}FORUM",
                columns: new[] { $"{_forumTablePrefix}ID", "F_ARCHIVE_SCHED", "F_A_COUNT", "F_A_TOPICS", "CAT_ID", "F_COUNT_M_POSTS", "F_DEFAULTDAYS", "F_DELETE_SCHED", "F_DESCRIPTION", "F_IP", "F_L_ARCHIVE", "F_L_DELETE", "F_LAST_POST", "F_LAST_POST_AUTHOR", "F_LAST_POST_REPLY_ID", "F_LAST_POST_TOPIC_ID", "F_MAIL", "F_MODERATION", "F_ORDER", "F_PASSWORD_NEW", "F_PRIVATEFORUMS", "F_COUNT", "F_STATUS", "F_SUBSCRIPTION", "F_SUBJECT", "F_TOPICS", "F_TYPE", "F_URL" },
                values: new object[] { 1, 0, 0, 0, 1, (short)1, 30, 0, "This forum gives you a chance to become more familiar with how this product responds to different features and keeps testing in one place instead of posting tests all over. Happy Posting! [:)]", null, null, null, null, null, null, null, (short)0, 0, 0, null, 0, 0, (short)1, 0, "Testing Forums", 0, (short)0, null });

            migrationBuilder.InsertData(
                table: $"{_forumTablePrefix}NAMEFILTER",
                columns: new[] { "N_ID", "N_NAME" },
                values: new object[] { 1, "Administrator" });

            migrationBuilder.InsertData(
                table: $"{_forumTablePrefix}TOTALS",
                columns: new[] { "COUNT_ID", "P_A_COUNT", "T_A_COUNT", "P_COUNT", "T_COUNT", "U_COUNT" },
                values: new object[] { (short)1, 0, 0, 1, 1, 0 });


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
