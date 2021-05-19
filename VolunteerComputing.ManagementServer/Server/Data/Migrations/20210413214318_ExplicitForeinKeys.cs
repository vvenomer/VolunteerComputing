using Microsoft.EntityFrameworkCore.Migrations;

namespace VolunteerComputing.ManagementServer.Server.Data.Migrations
{
    public partial class ExplicitForeinKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PacketTypes_ComputeTask_ComputeTaskId",
                table: "PacketTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_PacketTypes_ComputeTask_ComputeTaskId1",
                table: "PacketTypes");

            migrationBuilder.RenameColumn(
                name: "ComputeTaskId1",
                table: "PacketTypes",
                newName: "OutputComputeTaskId");

            migrationBuilder.RenameColumn(
                name: "ComputeTaskId",
                table: "PacketTypes",
                newName: "InputComputeTaskId");

            migrationBuilder.RenameIndex(
                name: "IX_PacketTypes_ComputeTaskId1",
                table: "PacketTypes",
                newName: "IX_PacketTypes_OutputComputeTaskId");

            migrationBuilder.RenameIndex(
                name: "IX_PacketTypes_ComputeTaskId",
                table: "PacketTypes",
                newName: "IX_PacketTypes_InputComputeTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_PacketTypes_ComputeTask_InputComputeTaskId",
                table: "PacketTypes",
                column: "InputComputeTaskId",
                principalTable: "ComputeTask",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PacketTypes_ComputeTask_OutputComputeTaskId",
                table: "PacketTypes",
                column: "OutputComputeTaskId",
                principalTable: "ComputeTask",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PacketTypes_ComputeTask_InputComputeTaskId",
                table: "PacketTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_PacketTypes_ComputeTask_OutputComputeTaskId",
                table: "PacketTypes");

            migrationBuilder.RenameColumn(
                name: "OutputComputeTaskId",
                table: "PacketTypes",
                newName: "ComputeTaskId1");

            migrationBuilder.RenameColumn(
                name: "InputComputeTaskId",
                table: "PacketTypes",
                newName: "ComputeTaskId");

            migrationBuilder.RenameIndex(
                name: "IX_PacketTypes_OutputComputeTaskId",
                table: "PacketTypes",
                newName: "IX_PacketTypes_ComputeTaskId1");

            migrationBuilder.RenameIndex(
                name: "IX_PacketTypes_InputComputeTaskId",
                table: "PacketTypes",
                newName: "IX_PacketTypes_ComputeTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_PacketTypes_ComputeTask_ComputeTaskId",
                table: "PacketTypes",
                column: "ComputeTaskId",
                principalTable: "ComputeTask",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PacketTypes_ComputeTask_ComputeTaskId1",
                table: "PacketTypes",
                column: "ComputeTaskId1",
                principalTable: "ComputeTask",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
