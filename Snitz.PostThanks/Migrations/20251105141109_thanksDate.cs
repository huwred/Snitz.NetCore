using Microsoft.EntityFrameworkCore.Migrations;
using SnitzCore.Data.Extensions;

#nullable disable

namespace Snitz.PostThanks.Migrations
{
    /// <inheritdoc />
    public partial class thanksDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.TableExists("FORUM_THANKS"))
            {
                migrationBuilder.AddColumn<string>(
                name: "THANKS_DATE",
                table: "FORUM_THANKS",
                type: "varchar(16)",
                nullable: true,
                defaultValue: "");
            }

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
 
        }
    }
}
