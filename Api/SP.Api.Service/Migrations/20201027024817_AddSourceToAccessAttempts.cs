using Microsoft.EntityFrameworkCore.Migrations;

namespace SP.Api.Service.Migrations
{
    public partial class AddSourceToAccessAttempts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Access.Attempts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Source",
                table: "Access.Attempts");
        }
    }
}
