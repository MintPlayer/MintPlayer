using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MintPlayer.Data.Migrations
{
    public partial class AddMediumTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MediumTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(nullable: true),
                    PlayerType = table.Column<int>(nullable: false),
                    UserInsertId = table.Column<Guid>(nullable: true),
                    UserUpdateId = table.Column<Guid>(nullable: true),
                    UserDeleteId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediumTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediumTypes_AspNetUsers_UserDeleteId",
                        column: x => x.UserDeleteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MediumTypes_AspNetUsers_UserInsertId",
                        column: x => x.UserInsertId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MediumTypes_AspNetUsers_UserUpdateId",
                        column: x => x.UserUpdateId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MediumTypes_UserDeleteId",
                table: "MediumTypes",
                column: "UserDeleteId");

            migrationBuilder.CreateIndex(
                name: "IX_MediumTypes_UserInsertId",
                table: "MediumTypes",
                column: "UserInsertId");

            migrationBuilder.CreateIndex(
                name: "IX_MediumTypes_UserUpdateId",
                table: "MediumTypes",
                column: "UserUpdateId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MediumTypes");
        }
    }
}
