using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SP.API.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "Blocks",
                table => new
                {
                    Id = table.Column<long>()
                        .Annotation("Sqlite:Autoincrement", true),
                    IpAddress = table.Column<string>(nullable: true),
                    Hostname = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(),
                    Country = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    ISP = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Blocks", x => x.Id); });

            migrationBuilder.CreateTable(
                "Login.Attempts",
                table => new
                {
                    Id = table.Column<long>()
                        .Annotation("Sqlite:Autoincrement", true),
                    IpAddress = table.Column<string>(nullable: true),
                    EventDate = table.Column<DateTime>(),
                    Details = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Login.Attempts", x => x.Id); });

            migrationBuilder.CreateTable(
                "Statistics.Blocks",
                table => new
                {
                    Id = table.Column<long>()
                        .Annotation("Sqlite:Autoincrement", true),
                    Country = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    ISP = table.Column<string>(nullable: true),
                    Attempts = table.Column<long>()
                },
                constraints: table => { table.PrimaryKey("PK_Statistics.Blocks", x => x.Id); });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "Blocks");

            migrationBuilder.DropTable(
                "Login.Attempts");

            migrationBuilder.DropTable(
                "Statistics.Blocks");
        }
    }
}