using Microsoft.EntityFrameworkCore.Migrations;

namespace RobiGroup.AskMeFootball.Data.Migrations
{
    public partial class LanguageQuestion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Text",
                table: "Questions",
                newName: "TextRu");

            migrationBuilder.RenameColumn(
                name: "Text",
                table: "QuestionAnswers",
                newName: "TextRu");

            migrationBuilder.AddColumn<string>(
                name: "TextKz",
                table: "Questions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TextKz",
                table: "QuestionAnswers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TextKz",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "TextKz",
                table: "QuestionAnswers");

            migrationBuilder.RenameColumn(
                name: "TextRu",
                table: "Questions",
                newName: "Text");

            migrationBuilder.RenameColumn(
                name: "TextRu",
                table: "QuestionAnswers",
                newName: "Text");
        }
    }
}
