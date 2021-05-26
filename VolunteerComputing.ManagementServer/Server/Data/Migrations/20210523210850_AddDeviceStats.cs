using Microsoft.EntityFrameworkCore.Migrations;

namespace VolunteerComputing.ManagementServer.Server.Data.Migrations
{
    public partial class AddDeviceStats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Count = table.Column<int>(type: "int", nullable: false),
                    TimeSum = table.Column<double>(type: "float", nullable: false),
                    EnergySum = table.Column<double>(type: "float", nullable: false),
                    IsCpu = table.Column<bool>(type: "bit", nullable: false),
                    ComputeTaskId = table.Column<int>(type: "int", nullable: true),
                    DeviceDataId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceStats_ComputeTask_ComputeTaskId",
                        column: x => x.ComputeTaskId,
                        principalTable: "ComputeTask",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeviceStats_Devices_DeviceDataId",
                        column: x => x.DeviceDataId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceStats_ComputeTaskId",
                table: "DeviceStats",
                column: "ComputeTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceStats_DeviceDataId",
                table: "DeviceStats",
                column: "DeviceDataId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceStats");
        }
    }
}
