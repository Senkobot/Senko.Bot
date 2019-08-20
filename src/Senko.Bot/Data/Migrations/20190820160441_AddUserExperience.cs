using Microsoft.EntityFrameworkCore.Migrations;

namespace Senko.Bot.Migrations
{
    public partial class AddUserExperience : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserExperience",
                columns: table => new
                {
                    UserId = table.Column<decimal>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false),
                    Experience = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserExperience", x => new { x.GuildId, x.UserId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserExperience");
        }
    }
}
