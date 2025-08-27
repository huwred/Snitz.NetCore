using Microsoft.EntityFrameworkCore.Migrations;
using SnitzCore.Data.Extensions;
using System;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class latestchages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (!migrationBuilder.ColumnExists("FORUM_TOPICS", "T_ANSWERED"))
            {
            migrationBuilder.AddColumn<bool>(
                name: "T_ANSWERED",
                table: "FORUM_TOPICS",
                type: "bit",
                nullable: false,
                defaultValue: false);
            }

            if (!migrationBuilder.ColumnExists("FORUM_REPLY", "R_ANSWER"))
            {
            migrationBuilder.AddColumn<bool>(
                name: "R_ANSWER",
                table: "FORUM_REPLY",
                type: "bit",
                nullable: false,
                defaultValue: false);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
//dummy
        }
    }
}
