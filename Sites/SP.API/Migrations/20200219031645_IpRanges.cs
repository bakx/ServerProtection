using Microsoft.EntityFrameworkCore.Migrations;

namespace SP.API.Migrations
{
    public partial class IpRanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                "IpAddress1",
                "Login.Attempts",
                nullable: false,
                defaultValue: (byte) 0);

            migrationBuilder.AddColumn<byte>(
                "IpAddress2",
                "Login.Attempts",
                nullable: false,
                defaultValue: (byte) 0);

            migrationBuilder.AddColumn<byte>(
                "IpAddress3",
                "Login.Attempts",
                nullable: false,
                defaultValue: (byte) 0);

            migrationBuilder.AddColumn<byte>(
                "IpAddress4",
                "Login.Attempts",
                nullable: false,
                defaultValue: (byte) 0);

            migrationBuilder.AddColumn<byte>(
                "IpAddress1",
                "Blocks",
                nullable: false,
                defaultValue: (byte) 0);

            migrationBuilder.AddColumn<byte>(
                "IpAddress2",
                "Blocks",
                nullable: false,
                defaultValue: (byte) 0);

            migrationBuilder.AddColumn<byte>(
                "IpAddress3",
                "Blocks",
                nullable: false,
                defaultValue: (byte) 0);

            migrationBuilder.AddColumn<byte>(
                "IpAddress4",
                "Blocks",
                nullable: false,
                defaultValue: (byte) 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                "IpAddress1",
                "Login.Attempts");

            migrationBuilder.DropColumn(
                "IpAddress2",
                "Login.Attempts");

            migrationBuilder.DropColumn(
                "IpAddress3",
                "Login.Attempts");

            migrationBuilder.DropColumn(
                "IpAddress4",
                "Login.Attempts");

            migrationBuilder.DropColumn(
                "IpAddress1",
                "Blocks");

            migrationBuilder.DropColumn(
                "IpAddress2",
                "Blocks");

            migrationBuilder.DropColumn(
                "IpAddress3",
                "Blocks");

            migrationBuilder.DropColumn(
                "IpAddress4",
                "Blocks");
        }
    }
}