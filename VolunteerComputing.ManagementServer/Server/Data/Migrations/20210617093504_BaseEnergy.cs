using Microsoft.EntityFrameworkCore.Migrations;

namespace VolunteerComputing.ManagementServer.Server.Data.Migrations
{
    public partial class BaseEnergy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "BaseCpuEnergyConsumption",
                table: "Devices",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BaseGpuEnergyConsumption",
                table: "Devices",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseCpuEnergyConsumption",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "BaseGpuEnergyConsumption",
                table: "Devices");
        }
    }
}
