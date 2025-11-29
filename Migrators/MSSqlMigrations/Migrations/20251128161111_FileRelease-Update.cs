using Microsoft.EntityFrameworkCore.Migrations;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Models;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class FileReleaseUpdate : SnitzMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            SetParameters();

            if (!migrationBuilder.ColumnExists("VisitorLog","UserName"))
            {
                migrationBuilder.AddColumn<string>(
                    name: "UserName",
                    table: "VisitorLog",
                    type: "nvarchar(max)",
                    nullable: true);    
            }

            if (!migrationBuilder.TableExists($"{_forumTablePrefix}FILECOUNT"))
            {
                migrationBuilder.CreateTable(
                    name: $"{_forumTablePrefix}FILECOUNT",
                    columns: table => new
                    {
                        FC_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        Posted = table.Column<string>(type: "nvarchar(max)", nullable: false),
                        LinkHits = table.Column<int>(type: "int", nullable: false),
                        LinkOrder = table.Column<int>(type: "int", nullable: false),
                        Archived = table.Column<int>(type: "int", nullable: false),
                        Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                        FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                        Version = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        ReadMe = table.Column<string>(type: "nvarchar(max)", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_FILECOUNT", x => x.FC_ID);
                    });
            }
            else{
                migrationBuilder.AddColumn<string>(
                    name: "ReadMe",
                    table: $"{_forumTablePrefix}FILECOUNT",
                    type: "nvarchar(max)",
                    nullable: true);            
                }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FORUM_FILECOUNT");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "VisitorLog");
        }
    }
}
