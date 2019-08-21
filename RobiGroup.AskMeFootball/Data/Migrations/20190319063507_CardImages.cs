using Microsoft.EntityFrameworkCore.Migrations;

namespace RobiGroup.AskMeFootball.Data.Migrations
{
    public partial class CardImages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Cards",
                newName: "ImageUrlSelect");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrlCard",
                table: "Cards",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrlDetail",
                table: "Cards",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrlCard",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "ImageUrlDetail",
                table: "Cards");

            migrationBuilder.RenameColumn(
                name: "ImageUrlSelect",
                table: "Cards",
                newName: "ImageUrl");
        }
    }
}
