using Microsoft.EntityFrameworkCore.Migrations;

namespace VolunteerComputing.ManagementServer.Server.Data.Migrations
{
    public partial class PacketTypesManyToMany : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PacketTypes_ComputeTask_InputComputeTaskId",
                table: "PacketTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_PacketTypes_ComputeTask_OutputComputeTaskId",
                table: "PacketTypes");

            migrationBuilder.DropIndex(
                name: "IX_PacketTypes_InputComputeTaskId",
                table: "PacketTypes");

            migrationBuilder.DropIndex(
                name: "IX_PacketTypes_OutputComputeTaskId",
                table: "PacketTypes");

            migrationBuilder.DropColumn(
                name: "InputComputeTaskId",
                table: "PacketTypes");

            migrationBuilder.DropColumn(
                name: "OutputComputeTaskId",
                table: "PacketTypes");

            migrationBuilder.CreateTable(
                name: "PacketTypeToComputeTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PacketTypeId = table.Column<int>(type: "int", nullable: true),
                    ComputeTaskId = table.Column<int>(type: "int", nullable: true),
                    IsInput = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PacketTypeToComputeTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PacketTypeToComputeTasks_ComputeTask_ComputeTaskId",
                        column: x => x.ComputeTaskId,
                        principalTable: "ComputeTask",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PacketTypeToComputeTasks_PacketTypes_PacketTypeId",
                        column: x => x.PacketTypeId,
                        principalTable: "PacketTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PacketTypeToComputeTasks_ComputeTaskId",
                table: "PacketTypeToComputeTasks",
                column: "ComputeTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_PacketTypeToComputeTasks_PacketTypeId",
                table: "PacketTypeToComputeTasks",
                column: "PacketTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PacketTypeToComputeTasks");

            migrationBuilder.AddColumn<int>(
                name: "InputComputeTaskId",
                table: "PacketTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OutputComputeTaskId",
                table: "PacketTypes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PacketTypes_InputComputeTaskId",
                table: "PacketTypes",
                column: "InputComputeTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_PacketTypes_OutputComputeTaskId",
                table: "PacketTypes",
                column: "OutputComputeTaskId");

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
    }
}
