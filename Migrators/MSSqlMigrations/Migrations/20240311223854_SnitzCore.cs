﻿using System;
using System.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using SnitzCore.Data;

#nullable disable

namespace Migrations
{
    /// <inheritdoc />
    public partial class SnitzCore : Migration
    {
        private static bool Exists(string tableName)
        {
            var connstring = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["SnitzConnection"];
            int count = 0;
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = connstring;
                conn.Open();
  
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT COUNT(*) FROM sys.tables WHERE name = '{tableName}'";
  
                count = (int)cmd.ExecuteScalar();
                conn.Close();
                return count > 0;
            }

        }
        private static bool ColumnExists(string tableName, string column)
        {
            var connstring = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["SnitzConnection"];
            int count = 0;
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = connstring;
                conn.Open();
  
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT COUNT(*) FROM sys.columns WHERE Name = N'{column}' AND Object_ID = Object_ID(N'{tableName}')";
  
                count = (int)cmd.ExecuteScalar();
                conn.Close();
                return count > 0;
            }

        }

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (!ColumnExists("FORUM_TOPICS", "T_ANSWERED"))
            {
            migrationBuilder.AddColumn<bool>(
                name: "T_ANSWERED",
                table: "FORUM_TOPICS",
                type: "bit",
                nullable: false,
                defaultValue: false);
            }

            if (!ColumnExists("FORUM_REPLY", "R_ANSWER"))
            {
            migrationBuilder.AddColumn<bool>(
                name: "R_ANSWER",
                table: "FORUM_REPLY",
                type: "bit",
                nullable: false,
                defaultValue: false);
            }


            if (!Exists("FORUM_BOOKMARKS"))
            {
                migrationBuilder.CreateTable(
                    name: "FORUM_BOOKMARKS",
                    columns: table => new
                    {
                        BOOKMARK_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        B_TOPICID = table.Column<int>(type: "int", nullable: false),
                        B_MEMBERID = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_BOOKMARKS", x => x.BOOKMARK_ID);
                        table.ForeignKey(
                            name: "FK_FORUM_BOOKMARKS_FORUM_MEMBERS_B_MEMBERID",
                            column: x => x.B_MEMBERID,
                            principalTable: "FORUM_MEMBERS",
                            principalColumn: "MEMBER_ID",
                            onDelete: ReferentialAction.Cascade);
                        table.ForeignKey(
                            name: "FK_FORUM_BOOKMARKS_FORUM_TOPICS_B_TOPICID",
                            column: x => x.B_TOPICID,
                            principalTable: "FORUM_TOPICS",
                            principalColumn: "TOPIC_ID",
                            onDelete: ReferentialAction.Cascade);
                    });
                migrationBuilder.CreateIndex(
                    name: "IX_FORUM_BOOKMARKS_B_MEMBERID",
                    table: "FORUM_BOOKMARKS",
                    column: "B_MEMBERID");

                migrationBuilder.CreateIndex(
                    name: "IX_FORUM_BOOKMARKS_B_TOPICID",
                    table: "FORUM_BOOKMARKS",
                    column: "B_TOPICID");
            }

            if (!Exists("FORUM_IMAGES"))
            {
                migrationBuilder.CreateTable(
                    name: "FORUM_IMAGE_CAT",
                    columns: table => new
                    {
                        CAT_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        MEMBER_ID = table.Column<int>(type: "int", nullable: false),
                        CAT_DESC = table.Column<string>(type: "nvarchar(max)", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_IMAGE_CAT", x => x.CAT_ID);
                    });

                migrationBuilder.CreateTable(
                    name: "FORUM_ORG_GROUP",
                    columns: table => new
                    {
                        O_GROUP_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        O_GROUP_ORDER = table.Column<int>(type: "int", nullable: false),
                        O_GROUP_NAME = table.Column<string>(type: "nvarchar(max)", nullable: true)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_ORG_GROUP", x => x.O_GROUP_ID);
                    });

                migrationBuilder.CreateTable(
                    name: "FORUM_IMAGES",
                    columns: table => new
                    {
                        I_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        I_MID = table.Column<int>(type: "int", nullable: false),
                        I_CAT = table.Column<int>(type: "int", nullable: true),
                        I_WIDTH = table.Column<int>(type: "int", nullable: true),
                        I_HEIGHT = table.Column<int>(type: "int", nullable: true),
                        I_SIZE = table.Column<int>(type: "int", nullable: false),
                        I_VIEWS = table.Column<int>(type: "int", nullable: true),
                        I_LOC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        I_DESC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        I_TYPE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        I_DATE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        I_GROUP_ID = table.Column<int>(type: "int", nullable: true),
                        I_SCIENTIFICNAME = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        I_NORWEGIANNAME = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        I_PRIVATE = table.Column<bool>(type: "bit", nullable: false),
                        I_NOTFEATURED = table.Column<bool>(type: "bit", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_IMAGES", x => x.I_ID);
                        table.ForeignKey(
                            name: "FK_FORUM_IMAGES_FORUM_IMAGE_CAT_I_CAT",
                            column: x => x.I_CAT,
                            principalTable: "FORUM_IMAGE_CAT",
                            principalColumn: "CAT_ID");
                        table.ForeignKey(
                            name: "FK_FORUM_IMAGES_FORUM_MEMBERS_I_MID",
                            column: x => x.I_MID,
                            principalTable: "FORUM_MEMBERS",
                            principalColumn: "MEMBER_ID",
                            onDelete: ReferentialAction.Cascade);
                        table.ForeignKey(
                            name: "FK_FORUM_IMAGES_FORUM_ORG_GROUP_I_GROUP_ID",
                            column: x => x.I_GROUP_ID,
                            principalTable: "FORUM_ORG_GROUP",
                            principalColumn: "O_GROUP_ID");
                    });


                migrationBuilder.CreateIndex(
                    name: "IX_FORUM_IMAGES_I_CAT",
                    table: "FORUM_IMAGES",
                    column: "I_CAT");

                migrationBuilder.CreateIndex(
                    name: "IX_FORUM_IMAGES_I_GROUP_ID",
                    table: "FORUM_IMAGES",
                    column: "I_GROUP_ID");

                migrationBuilder.CreateIndex(
                    name: "IX_FORUM_IMAGES_I_ID",
                    table: "FORUM_IMAGES",
                    column: "I_ID",
                    unique: true);

                migrationBuilder.CreateIndex(
                    name: "IX_FORUM_IMAGES_I_MID",
                    table: "FORUM_IMAGES",
                    column: "I_MID");
            }

            if (!Exists("FORUM_SPAM_MAIL"))
            {
                migrationBuilder.CreateTable(
                    name: "FORUM_SPAM_MAIL",
                    columns: table => new
                    {
                        SPAM_ID = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        SPAM_SERVER = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_FORUM_SPAM_MAIL", x => x.SPAM_ID);
                    });
            }

            if (!Exists("webpages_Membership"))
            {
                migrationBuilder.CreateTable(
                    name: "webpages_Membership",
                    columns: table => new
                    {
                        UserId = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                        Password = table.Column<string>(type: "nvarchar(75)", maxLength: 75, nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_webpages_Membership", x => x.UserId);
                    });

                migrationBuilder.CreateTable(
                    name: "webpages_Roles",
                    columns: table => new
                    {
                        RoleId = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        RoleName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_webpages_Roles", x => x.RoleId);
                    });

                migrationBuilder.CreateTable(
                    name: "webpages_UsersInRoles",
                    columns: table => new
                    {
                        UserId = table.Column<int>(type: "int", nullable: false),
                        RoleId = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.ForeignKey(
                            name: "FK_webpages_UsersInRoles_FORUM_MEMBERS_UserId",
                            column: x => x.UserId,
                            principalTable: "FORUM_MEMBERS",
                            principalColumn: "MEMBER_ID",
                            onDelete: ReferentialAction.Cascade);
                        table.ForeignKey(
                            name: "FK_webpages_UsersInRoles_webpages_Roles_RoleId",
                            column: x => x.RoleId,
                            principalTable: "webpages_Roles",
                            principalColumn: "RoleId",
                            onDelete: ReferentialAction.Cascade);
                    });



                migrationBuilder.CreateIndex(
                    name: "IX_webpages_UsersInRoles_RoleId",
                    table: "webpages_UsersInRoles",
                    column: "RoleId");

                migrationBuilder.CreateIndex(
                    name: "IX_webpages_UsersInRoles_UserId",
                    table: "webpages_UsersInRoles",
                    column: "UserId");
            }

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FORUM_BOOKMARKS");

            migrationBuilder.DropTable(
                name: "FORUM_IMAGES");

            migrationBuilder.DropTable(
                name: "FORUM_SPAM_MAIL");

            migrationBuilder.DropTable(
                name: "webpages_Membership");

            migrationBuilder.DropTable(
                name: "webpages_UsersInRoles");

            migrationBuilder.DropTable(
                name: "FORUM_IMAGE_CAT");

            migrationBuilder.DropTable(
                name: "FORUM_ORG_GROUP");

            migrationBuilder.DropTable(
                name: "webpages_Roles");

            migrationBuilder.DropColumn(
                name: "T_ANSWERED",
                table: "FORUM_TOPICS");

            migrationBuilder.DropColumn(
                name: "R_ANSWER",
                table: "FORUM_REPLY");

            migrationBuilder.AddColumn<int>(
                name: "LastPoster",
                table: "FORUM_TOPICS",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastPoster",
                table: "FORUM_A_TOPICS",
                type: "int",
                nullable: true);
        }
    }
}