using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Models;
using System;
using System.Reflection;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class MVCUpdate : SnitzMigration
    {

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            SetParameters();

            if(!migrationBuilder.TableExists($"webpages_Membership"))
            {
                migrationBuilder.CreateTable(
                name: "webpages_Membership",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", maxLength: 75, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_webpages_Membership", x => x.UserId);
                });
            }

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
