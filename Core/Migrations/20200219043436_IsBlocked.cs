using Microsoft.EntityFrameworkCore.Migrations;

namespace SP.Core.Migrations
{
    public partial class IsBlocked : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "IsBlocked",
                table: "Blocks",
                nullable: false,
                defaultValue: (byte)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBlocked",
                table: "Blocks");
        }
    }
}
