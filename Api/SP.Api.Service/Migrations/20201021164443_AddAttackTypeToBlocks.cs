using Microsoft.EntityFrameworkCore.Migrations;

namespace SP.Api.Service.Migrations
{
    public partial class AddAttackTypeToBlocks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AttackType",
                table: "Blocks",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttackType",
                table: "Blocks");
        }
    }
}
