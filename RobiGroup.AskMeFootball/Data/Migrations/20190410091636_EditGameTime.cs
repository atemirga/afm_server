using Microsoft.EntityFrameworkCore.Migrations;

namespace RobiGroup.AskMeFootball.Data.Migrations
{
    public partial class EditGameTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TwoHour",
                table: "Cards",
                newName: "IsTwoHour");

            migrationBuilder.RenameColumn(
                name: "HalfHour",
                table: "Cards",
                newName: "IsHalfHour");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsTwoHour",
                table: "Cards",
                newName: "TwoHour");

            migrationBuilder.RenameColumn(
                name: "IsHalfHour",
                table: "Cards",
                newName: "HalfHour");
        }
    }
}
