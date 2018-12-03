using Microsoft.EntityFrameworkCore.Migrations;

namespace RobiGroup.AskMeFootball.Data.Migrations
{
    public partial class AddedMatchReadyCancelColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Cancel",
                table: "MatchGamers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Ready",
                table: "MatchGamers",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cancel",
                table: "MatchGamers");

            migrationBuilder.DropColumn(
                name: "Ready",
                table: "MatchGamers");
        }
    }
}
