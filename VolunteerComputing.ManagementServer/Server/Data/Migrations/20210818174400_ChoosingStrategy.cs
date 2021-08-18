using Microsoft.EntityFrameworkCore.Migrations;

namespace VolunteerComputing.ManagementServer.Server.Data.Migrations
{
    public partial class ChoosingStrategy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ChanceToUseNewDevice",
                table: "Projects",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "ChoosingStrategy",
                table: "Projects",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChanceToUseNewDevice",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ChoosingStrategy",
                table: "Projects");
        }
    }
}
