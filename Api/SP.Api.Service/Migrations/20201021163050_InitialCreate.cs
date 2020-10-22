using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SP.Api.Service.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Access.Attempts",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IpAddress = table.Column<string>(nullable: true),
                    IpAddress1 = table.Column<byte>(nullable: false),
                    IpAddress2 = table.Column<byte>(nullable: false),
                    IpAddress3 = table.Column<byte>(nullable: false),
                    IpAddress4 = table.Column<byte>(nullable: false),
                    EventDate = table.Column<DateTime>(nullable: false),
                    Details = table.Column<string>(nullable: true),
                    AttackType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Access.Attempts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Blocks",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IpAddress = table.Column<string>(nullable: true),
                    IpAddress1 = table.Column<byte>(nullable: false),
                    IpAddress2 = table.Column<byte>(nullable: false),
                    IpAddress3 = table.Column<byte>(nullable: false),
                    IpAddress4 = table.Column<byte>(nullable: false),
                    Hostname = table.Column<string>(nullable: true),
                    Country = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    ISP = table.Column<string>(nullable: true),
                    Details = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    FirewallRuleName = table.Column<string>(nullable: true),
                    IsBlocked = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Statistics.Blocks",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Country = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    ISP = table.Column<string>(nullable: true),
                    Attempts = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statistics.Blocks", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Access.Attempts");

            migrationBuilder.DropTable(
                name: "Blocks");

            migrationBuilder.DropTable(
                name: "Statistics.Blocks");
        }
    }
}
