using Microsoft.EntityFrameworkCore.Migrations;

namespace SP.Core.Migrations
{
    public partial class FirewallRuleName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                "FirewallRuleName",
                "Blocks",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                "FirewallRuleName",
                "Blocks");
        }
    }
}