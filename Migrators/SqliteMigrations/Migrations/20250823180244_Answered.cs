using Microsoft.EntityFrameworkCore.Migrations;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Models;
using System;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class Answered : SnitzMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            SetParameters();
            if (!migrationBuilder.ColumnExists($"{_forumTablePrefix}TOPICS", "T_ANSWERED"))
            {
            migrationBuilder.AddColumn<bool>(
                name: "T_ANSWERED",
                table: $"{_forumTablePrefix}TOPICS",
                type: "bit",
                nullable: false,
                defaultValue: false);
            }

            if (!migrationBuilder.ColumnExists($"{_forumTablePrefix}REPLY", "R_ANSWER"))
            {
            migrationBuilder.AddColumn<bool>(
                name: "R_ANSWER",
                table: $"{_forumTablePrefix}REPLY",
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
