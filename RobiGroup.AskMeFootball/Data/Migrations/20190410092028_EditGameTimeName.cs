using Microsoft.EntityFrameworkCore.Migrations;

namespace RobiGroup.AskMeFootball.Data.Migrations
{
    public partial class EditGameTimeName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsTwoHour",
                table: "Cards",
                newName: "IsTwoH");

            migrationBuilder.RenameColumn(
                name: "IsHalfHour",
                table: "Cards",
                newName: "IsHalfH");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsTwoH",
                table: "Cards",
                newName: "IsTwoHour");

            migrationBuilder.RenameColumn(
                name: "IsHalfH",
                table: "Cards",
                newName: "IsHalfHour");
        }
    }
}
