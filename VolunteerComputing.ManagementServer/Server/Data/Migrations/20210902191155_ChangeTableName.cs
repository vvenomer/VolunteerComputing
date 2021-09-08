using Microsoft.EntityFrameworkCore.Migrations;

namespace VolunteerComputing.ManagementServer.Server.Data.Migrations
{
    public partial class ChangeTableName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bundles_ComputeTask_ComputeTaskId",
                table: "Bundles");

            migrationBuilder.DropForeignKey(
                name: "FK_ComputeTask_Projects_ProjectId",
                table: "ComputeTask");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceStats_ComputeTask_ComputeTaskId",
                table: "DeviceStats");

            migrationBuilder.DropForeignKey(
                name: "FK_PacketTypeToComputeTasks_ComputeTask_ComputeTaskId",
                table: "PacketTypeToComputeTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Result_Projects_ProjectId",
                table: "Result");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Result",
                table: "Result");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ComputeTask",
                table: "ComputeTask");

            migrationBuilder.RenameTable(
                name: "Result",
                newName: "Results");

            migrationBuilder.RenameTable(
                name: "ComputeTask",
                newName: "ComputeTasks");

            migrationBuilder.RenameIndex(
                name: "IX_Result_ProjectId",
                table: "Results",
                newName: "IX_Results_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_ComputeTask_ProjectId",
                table: "ComputeTasks",
                newName: "IX_ComputeTasks_ProjectId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Results",
                table: "Results",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ComputeTasks",
                table: "ComputeTasks",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bundles_ComputeTasks_ComputeTaskId",
                table: "Bundles",
                column: "ComputeTaskId",
                principalTable: "ComputeTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ComputeTasks_Projects_ProjectId",
                table: "ComputeTasks",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceStats_ComputeTasks_ComputeTaskId",
                table: "DeviceStats",
                column: "ComputeTaskId",
                principalTable: "ComputeTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PacketTypeToComputeTasks_ComputeTasks_ComputeTaskId",
                table: "PacketTypeToComputeTasks",
                column: "ComputeTaskId",
                principalTable: "ComputeTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Results_Projects_ProjectId",
                table: "Results",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bundles_ComputeTasks_ComputeTaskId",
                table: "Bundles");

            migrationBuilder.DropForeignKey(
                name: "FK_ComputeTasks_Projects_ProjectId",
                table: "ComputeTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceStats_ComputeTasks_ComputeTaskId",
                table: "DeviceStats");

            migrationBuilder.DropForeignKey(
                name: "FK_PacketTypeToComputeTasks_ComputeTasks_ComputeTaskId",
                table: "PacketTypeToComputeTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Results_Projects_ProjectId",
                table: "Results");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Results",
                table: "Results");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ComputeTasks",
                table: "ComputeTasks");

            migrationBuilder.RenameTable(
                name: "Results",
                newName: "Result");

            migrationBuilder.RenameTable(
                name: "ComputeTasks",
                newName: "ComputeTask");

            migrationBuilder.RenameIndex(
                name: "IX_Results_ProjectId",
                table: "Result",
                newName: "IX_Result_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_ComputeTasks_ProjectId",
                table: "ComputeTask",
                newName: "IX_ComputeTask_ProjectId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Result",
                table: "Result",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ComputeTask",
                table: "ComputeTask",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bundles_ComputeTask_ComputeTaskId",
                table: "Bundles",
                column: "ComputeTaskId",
                principalTable: "ComputeTask",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ComputeTask_Projects_ProjectId",
                table: "ComputeTask",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceStats_ComputeTask_ComputeTaskId",
                table: "DeviceStats",
                column: "ComputeTaskId",
                principalTable: "ComputeTask",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PacketTypeToComputeTasks_ComputeTask_ComputeTaskId",
                table: "PacketTypeToComputeTasks",
                column: "ComputeTaskId",
                principalTable: "ComputeTask",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Result_Projects_ProjectId",
                table: "Result",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
