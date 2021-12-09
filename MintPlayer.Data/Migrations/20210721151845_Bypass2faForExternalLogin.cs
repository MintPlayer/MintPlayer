using Microsoft.EntityFrameworkCore.Migrations;

namespace MintPlayer.Data.Migrations
{
    public partial class Bypass2faForExternalLogin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Bypass2faForExternalLogin",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bypass2faForExternalLogin",
                table: "AspNetUsers");
        }
    }
}
