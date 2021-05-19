using Microsoft.EntityFrameworkCore.Migrations;

namespace VolunteerComputing.ManagementServer.Server.Data.Migrations
{
    public partial class RemoveAggregatingEverySomeSeconds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgregateEverySeconds",
                table: "PacketTypes");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgregateEverySeconds",
                table: "PacketTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
