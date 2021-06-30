using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VolunteerComputing.ManagementServer.Server.Data.Migrations
{
    public partial class AddBundles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CpuWorks",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "GpuWorks",
                table: "Devices");

            migrationBuilder.AddColumn<int>(
                name: "MinAgreeingClients",
                table: "Projects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BundleId",
                table: "Packets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BundleResultId",
                table: "Packets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeviceWorkedOnItId",
                table: "Packets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CpuWorksOnBundle",
                table: "Devices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GpuWorksOnBundle",
                table: "Devices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "ComputeTask",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Bundles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimesSent = table.Column<int>(type: "int", nullable: false),
                    UntilCheck = table.Column<int>(type: "int", nullable: false),
                    ComputeTaskId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bundles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bundles_ComputeTask_ComputeTaskId",
                        column: x => x.ComputeTaskId,
                        principalTable: "ComputeTask",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BundleResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataHash = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    BundleId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BundleResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BundleResults_Bundles_BundleId",
                        column: x => x.BundleId,
                        principalTable: "Bundles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Packets_BundleId",
                table: "Packets",
                column: "BundleId");

            migrationBuilder.CreateIndex(
                name: "IX_Packets_BundleResultId",
                table: "Packets",
                column: "BundleResultId");

            migrationBuilder.CreateIndex(
                name: "IX_Packets_DeviceWorkedOnItId",
                table: "Packets",
                column: "DeviceWorkedOnItId");

            migrationBuilder.CreateIndex(
                name: "IX_ComputeTask_ProjectId",
                table: "ComputeTask",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_BundleResults_BundleId",
                table: "BundleResults",
                column: "BundleId");

            migrationBuilder.CreateIndex(
                name: "IX_Bundles_ComputeTaskId",
                table: "Bundles",
                column: "ComputeTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_ComputeTask_Projects_ProjectId",
                table: "ComputeTask",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Packets_BundleResults_BundleResultId",
                table: "Packets",
                column: "BundleResultId",
                principalTable: "BundleResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Packets_Bundles_BundleId",
                table: "Packets",
                column: "BundleId",
                principalTable: "Bundles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Packets_Devices_DeviceWorkedOnItId",
                table: "Packets",
                column: "DeviceWorkedOnItId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComputeTask_Projects_ProjectId",
                table: "ComputeTask");

            migrationBuilder.DropForeignKey(
                name: "FK_Packets_BundleResults_BundleResultId",
                table: "Packets");

            migrationBuilder.DropForeignKey(
                name: "FK_Packets_Bundles_BundleId",
                table: "Packets");

            migrationBuilder.DropForeignKey(
                name: "FK_Packets_Devices_DeviceWorkedOnItId",
                table: "Packets");

            migrationBuilder.DropTable(
                name: "BundleResults");

            migrationBuilder.DropTable(
                name: "Bundles");

            migrationBuilder.DropIndex(
                name: "IX_Packets_BundleId",
                table: "Packets");

            migrationBuilder.DropIndex(
                name: "IX_Packets_BundleResultId",
                table: "Packets");

            migrationBuilder.DropIndex(
                name: "IX_Packets_DeviceWorkedOnItId",
                table: "Packets");

            migrationBuilder.DropIndex(
                name: "IX_ComputeTask_ProjectId",
                table: "ComputeTask");

            migrationBuilder.DropColumn(
                name: "MinAgreeingClients",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "BundleId",
                table: "Packets");

            migrationBuilder.DropColumn(
                name: "BundleResultId",
                table: "Packets");

            migrationBuilder.DropColumn(
                name: "DeviceWorkedOnItId",
                table: "Packets");

            migrationBuilder.DropColumn(
                name: "CpuWorksOnBundle",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "GpuWorksOnBundle",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "ComputeTask");

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
        }
    }
}
