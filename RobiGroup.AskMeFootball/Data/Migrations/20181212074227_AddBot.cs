using Microsoft.EntityFrameworkCore.Migrations;

namespace RobiGroup.AskMeFootball.Data.Migrations
{
    public partial class AddBot : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Bot",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bot",
                table: "AspNetUsers");
        }
    }
}
