using Microsoft.EntityFrameworkCore.Migrations;

namespace VolunteerComputing.ManagementServer.Server.Data.Migrations
{
    public partial class AddComputeTasksAndPackets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComputeTask",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Program = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComputeTask", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PacketTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aggregable = table.Column<bool>(type: "bit", nullable: false),
                    AgregateEverySeconds = table.Column<int>(type: "int", nullable: false),
                    ComputeTaskId = table.Column<int>(type: "int", nullable: true),
                    ComputeTaskId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PacketTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PacketTypes_ComputeTask_ComputeTaskId",
                        column: x => x.ComputeTaskId,
                        principalTable: "ComputeTask",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PacketTypes_ComputeTask_ComputeTaskId1",
                        column: x => x.ComputeTaskId1,
                        principalTable: "ComputeTask",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Packets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeId = table.Column<int>(type: "int", nullable: true),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Packets_PacketTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "PacketTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Packets_TypeId",
                table: "Packets",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PacketTypes_ComputeTaskId",
                table: "PacketTypes",
                column: "ComputeTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_PacketTypes_ComputeTaskId1",
                table: "PacketTypes",
                column: "ComputeTaskId1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Packets");

            migrationBuilder.DropTable(
                name: "PacketTypes");

            migrationBuilder.DropTable(
                name: "ComputeTask");
        }
    }
}
