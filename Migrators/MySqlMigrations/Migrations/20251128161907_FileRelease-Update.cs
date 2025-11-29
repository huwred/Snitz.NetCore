using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Models;
using System;

#nullable disable

namespace MySqlMigrations.Migrations
{
    /// <inheritdoc />
    public partial class FileReleaseUpdate : SnitzMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            SetParameters();
            if (!migrationBuilder.TableExists($"{_forumTablePrefix}FILECOUNT"))
            {
                migrationBuilder.CreateTable(
                name: "FORUM_FILECOUNT",
                columns: table => new
                {
                    FC_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Posted = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LinkHits = table.Column<int>(type: "int", nullable: false),
                    LinkOrder = table.Column<int>(type: "int", nullable: false),
                    Archived = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Version = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReadMe = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORUM_FILECOUNT", x => x.FC_ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
            }
            else
            {
                    migrationBuilder.AddColumn<string>(
                    name: "ReadMe",
                    table: $"{_forumTablePrefix}FILECOUNT",
                    type: "longtext",
                    nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4");
            }

            if (!migrationBuilder.TableExists($"VisitorLog"))
            {
                migrationBuilder.CreateTable(
                name: "VisitorLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VisitTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IpAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserAgent = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Path = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitorLog", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
            }
            else
            {
                if (!migrationBuilder.ColumnExists("VisitorLog", "UserName"))
                {
                    migrationBuilder.AddColumn<string>(
                        name: "UserName",
                        table: "VisitorLog",
                        type: "longtext",
                        nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4");
                }
            }

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FORUM_FILECOUNT");

            migrationBuilder.DropTable(
                name: "VisitorLog");

            migrationBuilder.AlterColumn<short>(
                name: "COUNT_ID",
                table: "FORUM_TOTALS",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "RANK_ID",
                table: "FORUM_RANKING",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "T_UREPLIES",
                table: "FORUM_A_TOPICS",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "T_RATING_TOTAL_COUNT",
                table: "FORUM_A_TOPICS",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "T_RATING_TOTAL",
                table: "FORUM_A_TOPICS",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "T_POLLSTATUS",
                table: "FORUM_A_TOPICS",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "T_MAIL",
                table: "FORUM_A_TOPICS",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "T_ISPOLL",
                table: "FORUM_A_TOPICS",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "T_ALLOW_RATING",
                table: "FORUM_A_TOPICS",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "R_RATING",
                table: "FORUM_A_REPLY",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "FORUM_A_REPLY",
                keyColumn: "R_MESSAGE",
                keyValue: null,
                column: "R_MESSAGE",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "R_MESSAGE",
                table: "FORUM_A_REPLY",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<short>(
                name: "R_MAIL",
                table: "FORUM_A_REPLY",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "FORUM_A_REPLY",
                keyColumn: "R_DATE",
                keyValue: null,
                column: "R_DATE",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "R_DATE",
                table: "FORUM_A_REPLY",
                type: "varchar(14)",
                maxLength: 14,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(14)",
                oldMaxLength: 14,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
