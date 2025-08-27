using System;
using Microsoft.EntityFrameworkCore.Migrations;
using SnitzCore.Data.Extensions;

#nullable disable

namespace Snitz.Events.Migrations
{
    /// <inheritdoc />
    public partial class ForumUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (!migrationBuilder.ColumnExists("FORUM_FORUM", "F_ALLOWEVENTS"))
            {
                migrationBuilder.AddColumn<int>(
                    name: "F_ALLOWEVENTS",
                    table: "FORUM_FORUM",
                    type: "int",
                    nullable: true);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
