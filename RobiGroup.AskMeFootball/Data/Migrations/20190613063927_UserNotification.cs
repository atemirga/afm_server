using Microsoft.EntityFrameworkCore.Migrations;

namespace RobiGroup.AskMeFootball.Data.Migrations
{
    public partial class UserNotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_U0serNotifications",
                table: "U0serNotifications");

            migrationBuilder.RenameTable(
                name: "U0serNotifications",
                newName: "UserNotifications");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserNotifications",
                table: "UserNotifications",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserNotifications",
                table: "UserNotifications");

            migrationBuilder.RenameTable(
                name: "UserNotifications",
                newName: "U0serNotifications");

            migrationBuilder.AddPrimaryKey(
                name: "PK_U0serNotifications",
                table: "U0serNotifications",
                column: "Id");
        }
    }
}
