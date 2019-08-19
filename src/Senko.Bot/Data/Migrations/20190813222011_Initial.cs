using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Senko.Bot.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChannelPermission",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(nullable: false),
                    ChannelId = table.Column<decimal>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Granted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelPermission", x => new { x.GuildId, x.ChannelId });
                    table.UniqueConstraint("AK_ChannelPermission_GuildId_ChannelId_Name", x => new { x.GuildId, x.ChannelId, x.Name });
                });

            migrationBuilder.CreateTable(
                name: "GuildModule",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildModule", x => x.GuildId);
                    table.UniqueConstraint("AK_GuildModule_GuildId_Name", x => new { x.GuildId, x.Name });
                });

            migrationBuilder.CreateTable(
                name: "RolePermission",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(nullable: false),
                    RoleId = table.Column<decimal>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Granted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermission", x => new { x.GuildId, x.RoleId });
                    table.UniqueConstraint("AK_RolePermission_GuildId_RoleId_Name", x => new { x.GuildId, x.RoleId, x.Name });
                });

            migrationBuilder.CreateTable(
                name: "UserPermission",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(nullable: false),
                    UserId = table.Column<decimal>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Granted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermission", x => new { x.GuildId, x.UserId });
                    table.UniqueConstraint("AK_UserPermission_GuildId_UserId_Name", x => new { x.GuildId, x.UserId, x.Name });
                });

            migrationBuilder.CreateTable(
                name: "UserWarning",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    UserId = table.Column<decimal>(nullable: false),
                    ModeratorId = table.Column<decimal>(nullable: false),
                    ConsoleMessageId = table.Column<decimal>(nullable: true),
                    ConsoleChannelId = table.Column<decimal>(nullable: true),
                    Reason = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWarning", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChannelPermission");

            migrationBuilder.DropTable(
                name: "GuildModule");

            migrationBuilder.DropTable(
                name: "RolePermission");

            migrationBuilder.DropTable(
                name: "UserPermission");

            migrationBuilder.DropTable(
                name: "UserWarning");
        }
    }
}
