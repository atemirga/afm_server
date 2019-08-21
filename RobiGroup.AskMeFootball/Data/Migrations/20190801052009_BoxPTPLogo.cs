using Microsoft.EntityFrameworkCore.Migrations;

namespace RobiGroup.AskMeFootball.Data.Migrations
{
    public partial class BoxPTPLogo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstTeamLogo",
                table: "CardTeams",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondTeamLogo",
                table: "CardTeams",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntryPoint",
                table: "Cards",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstTeamLogo",
                table: "CardTeams");

            migrationBuilder.DropColumn(
                name: "SecondTeamLogo",
                table: "CardTeams");

            migrationBuilder.DropColumn(
                name: "EntryPoint",
                table: "Cards");
        }
    }
}
