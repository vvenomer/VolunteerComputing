using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VolunteerComputing.ManagementServer.Server.Data.Migrations
{
    public partial class CalculationTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "SecondsElapsed",
                table: "Result",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "Projects",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecondsElapsed",
                table: "Result");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Projects");
        }
    }
}
