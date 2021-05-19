using Microsoft.EntityFrameworkCore.Migrations;

namespace VolunteerComputing.ManagementServer.Server.Data.Migrations
{
    public partial class SeperateCpuAndGpu : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Works",
                table: "Devices",
                newName: "IsWindows");

            migrationBuilder.RenameColumn(
                name: "WindowsProgram",
                table: "ComputeTask",
                newName: "WindowsGpuProgram");

            migrationBuilder.RenameColumn(
                name: "LinuxProgram",
                table: "ComputeTask",
                newName: "WindowsCpuProgram");

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "PacketTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Aggregated",
                table: "Packets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CpuWorks",
                table: "Devices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GpuWorks",
                table: "Devices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LinuxCpuProgram",
                table: "ComputeTask",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinuxGpuProgram",
                table: "ComputeTask",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PacketTypes_ProjectId",
                table: "PacketTypes",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_PacketTypes_Projects_ProjectId",
                table: "PacketTypes",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PacketTypes_Projects_ProjectId",
                table: "PacketTypes");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_PacketTypes_ProjectId",
                table: "PacketTypes");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "PacketTypes");

            migrationBuilder.DropColumn(
                name: "Aggregated",
                table: "Packets");

            migrationBuilder.DropColumn(
                name: "CpuWorks",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "GpuWorks",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "LinuxCpuProgram",
                table: "ComputeTask");

            migrationBuilder.DropColumn(
                name: "LinuxGpuProgram",
                table: "ComputeTask");

            migrationBuilder.RenameColumn(
                name: "IsWindows",
                table: "Devices",
                newName: "Works");

            migrationBuilder.RenameColumn(
                name: "WindowsGpuProgram",
                table: "ComputeTask",
                newName: "WindowsProgram");

            migrationBuilder.RenameColumn(
                name: "WindowsCpuProgram",
                table: "ComputeTask",
                newName: "LinuxProgram");
        }
    }
}
