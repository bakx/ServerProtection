using Microsoft.EntityFrameworkCore.Migrations;

namespace SP.Core.Migrations
{
    public partial class IpRanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "IpAddress1",
                table: "Login.Attempts",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "IpAddress2",
                table: "Login.Attempts",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "IpAddress3",
                table: "Login.Attempts",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "IpAddress4",
                table: "Login.Attempts",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "IpAddress1",
                table: "Blocks",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "IpAddress2",
                table: "Blocks",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "IpAddress3",
                table: "Blocks",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "IpAddress4",
                table: "Blocks",
                nullable: false,
                defaultValue: (byte)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpAddress1",
                table: "Login.Attempts");

            migrationBuilder.DropColumn(
                name: "IpAddress2",
                table: "Login.Attempts");

            migrationBuilder.DropColumn(
                name: "IpAddress3",
                table: "Login.Attempts");

            migrationBuilder.DropColumn(
                name: "IpAddress4",
                table: "Login.Attempts");

            migrationBuilder.DropColumn(
                name: "IpAddress1",
                table: "Blocks");

            migrationBuilder.DropColumn(
                name: "IpAddress2",
                table: "Blocks");

            migrationBuilder.DropColumn(
                name: "IpAddress3",
                table: "Blocks");

            migrationBuilder.DropColumn(
                name: "IpAddress4",
                table: "Blocks");
        }
    }
}
