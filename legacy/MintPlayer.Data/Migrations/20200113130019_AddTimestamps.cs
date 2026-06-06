using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MintPlayer.Data.Migrations
{
    public partial class AddTimestamps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateDelete",
                table: "Subjects",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateInsert",
                table: "Subjects",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateUpdate",
                table: "Subjects",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateDelete",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "DateInsert",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "DateUpdate",
                table: "Subjects");
        }
    }
}
