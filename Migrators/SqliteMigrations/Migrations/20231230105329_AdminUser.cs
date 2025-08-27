using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class AdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "0D1F96F3-A8BD-4348-AFA4-61B931BB3553", null, "Moderator", "MODERATOR" },
                    { "2c5e174e-3b0e-446f-86af-483d56fd7210", null, "Administrator", "ADMINISTRATOR" },
                    { "467DF002-6D82-4109-979A-76F01FA9D4CF", null, "ForumMember", "FORUMMEMBER" },
                    { "1F55ACDD-212C-4824-96B5-F10A05FE6563", null, "Visitor", "VISITOR" },
                    { "E5EA0F7E-62A5-4870-B90F-B62965EA075E", null, "SuperAdmin", "SUPERADMIN" }
                });

            migrationBuilder.InsertData( // Admin User with password Passw0rd!
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Discriminator", "Email", "EmailConfirmed", "IsActive", "IsAdmin", "LockoutEnabled", "LockoutEnd", "MemberId", "MemberSince", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "ProfileImageUrl", "Rating", "SecurityStamp", "TwoFactorEnabled", "UserDescription", "UserName" },
                values: new object[] { "8e445865-a24d-4543-a6c6-9443d048cdb9", 0, null, "ForumUser", "xxxx@example.com", true, false, true, false, null, 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "XXXX@EXAMPLE.COM", "ADMINISTRATOR", "AQAAAAIAAYagAAAAEGhGQ8XcU+/GeShLZcARJtfYaIxYhK1lwlajwWCVmUJd1fOAdBhW0GhasV0QmAfCqw==", "+111111111111", true, null, 0, "6efdd0a9-6c20-44ea-8a4d-dec3ebd2240b", false, null, "Administrator" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,] {
                    { "2c5e174e-3b0e-446f-86af-483d56fd7210", "8e445865-a24d-4543-a6c6-9443d048cdb9" },
                    { "E5EA0F7E-62A5-4870-B90F-B62965EA075E", "8e445865-a24d-4543-a6c6-9443d048cdb9" },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0D1F96F3-A8BD-4348-AFA4-61B931BB3553");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "467DF002-6D82-4109-979A-76F01FA9D4CF");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "2c5e174e-3b0e-446f-86af-483d56fd7210", "8e445865-a24d-4543-a6c6-9443d048cdb9" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2c5e174e-3b0e-446f-86af-483d56fd7210");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "8e445865-a24d-4543-a6c6-9443d048cdb9");

        }
    }
}
