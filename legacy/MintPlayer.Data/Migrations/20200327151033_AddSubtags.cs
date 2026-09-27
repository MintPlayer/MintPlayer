using Microsoft.EntityFrameworkCore.Migrations;

namespace MintPlayer.Data.Migrations
{
    public partial class AddSubtags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "Tags",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_ParentId",
                table: "Tags",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Tags_ParentId",
                table: "Tags",
                column: "ParentId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Tags_ParentId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_ParentId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Tags");
        }
    }
}
