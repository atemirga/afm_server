using Microsoft.EntityFrameworkCore.Migrations;

namespace RobiGroup.AskMeFootball.Data.Migrations
{
    public partial class AddMatchGamerBonus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Bonus",
                table: "MatchGamers",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bonus",
                table: "MatchGamers");
        }
    }
}
