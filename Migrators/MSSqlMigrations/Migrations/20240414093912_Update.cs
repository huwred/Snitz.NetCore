using Microsoft.EntityFrameworkCore.Migrations;
using SnitzCore.Data.Extensions;

#nullable disable

namespace Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (!migrationBuilder.ColumnExists("FORUM_MEMBERS", "M_LASTHEREDATE"))
            {
            migrationBuilder.AddColumn<bool>(
                name: "M_LASTHEREDATE",
                table: "FORUM_MEMBERS",
                type: "Varchar(14)",
                nullable: true);
            }

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
