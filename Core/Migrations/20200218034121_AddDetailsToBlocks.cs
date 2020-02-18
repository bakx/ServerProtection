using Microsoft.EntityFrameworkCore.Migrations;

namespace SP.Core.Migrations
{
    public partial class AddDetailsToBlocks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Details",
                table: "Blocks",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Details",
                table: "Blocks");
        }
    }
}
