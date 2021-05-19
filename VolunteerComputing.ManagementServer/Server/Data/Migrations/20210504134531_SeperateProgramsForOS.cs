using Microsoft.EntityFrameworkCore.Migrations;

namespace VolunteerComputing.ManagementServer.Server.Data.Migrations
{
    public partial class SeperateProgramsForOS : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Program",
                table: "ComputeTask",
                newName: "WindowsProgram");

            migrationBuilder.AddColumn<string>(
                name: "ExeFilename",
                table: "ComputeTask",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinuxProgram",
                table: "ComputeTask",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExeFilename",
                table: "ComputeTask");

            migrationBuilder.DropColumn(
                name: "LinuxProgram",
                table: "ComputeTask");

            migrationBuilder.RenameColumn(
                name: "WindowsProgram",
                table: "ComputeTask",
                newName: "Program");
        }
    }
}
