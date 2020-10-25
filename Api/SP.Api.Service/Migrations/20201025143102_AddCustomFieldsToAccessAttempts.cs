using Microsoft.EntityFrameworkCore.Migrations;

namespace SP.Api.Service.Migrations
{
    public partial class AddCustomFieldsToAccessAttempts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Custom1",
                table: "Access.Attempts",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Custom2",
                table: "Access.Attempts",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "Custom3",
                table: "Access.Attempts",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Custom1",
                table: "Access.Attempts");

            migrationBuilder.DropColumn(
                name: "Custom2",
                table: "Access.Attempts");

            migrationBuilder.DropColumn(
                name: "Custom3",
                table: "Access.Attempts");
        }
    }
}
