using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MintPlayer.Data.Migrations
{
    public partial class AddBlogPosts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlogPosts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(nullable: true),
                    Headline = table.Column<string>(nullable: true),
                    Body = table.Column<string>(nullable: true),
                    UserInsertId = table.Column<Guid>(nullable: true),
                    UserUpdateId = table.Column<Guid>(nullable: true),
                    UserDeleteId = table.Column<Guid>(nullable: true),
                    DateInsert = table.Column<DateTime>(nullable: false),
                    DateUpdate = table.Column<DateTime>(nullable: true),
                    DateDelete = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogPosts_AspNetUsers_UserDeleteId",
                        column: x => x.UserDeleteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BlogPosts_AspNetUsers_UserInsertId",
                        column: x => x.UserInsertId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BlogPosts_AspNetUsers_UserUpdateId",
                        column: x => x.UserUpdateId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_UserDeleteId",
                table: "BlogPosts",
                column: "UserDeleteId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_UserInsertId",
                table: "BlogPosts",
                column: "UserInsertId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_UserUpdateId",
                table: "BlogPosts",
                column: "UserUpdateId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlogPosts");
        }
    }
}
