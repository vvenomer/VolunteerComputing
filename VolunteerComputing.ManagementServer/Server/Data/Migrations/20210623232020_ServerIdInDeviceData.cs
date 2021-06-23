using Microsoft.EntityFrameworkCore.Migrations;

namespace VolunteerComputing.ManagementServer.Server.Data.Migrations
{
    public partial class ServerIdInDeviceData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TaskServerId",
                table: "Devices",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaskServerId",
                table: "Devices");
        }
    }
}
