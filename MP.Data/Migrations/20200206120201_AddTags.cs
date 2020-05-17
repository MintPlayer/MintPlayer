using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MintPlayer.Data.Migrations
{
    public partial class AddTags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TagCategories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Color = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    UserInsertId = table.Column<Guid>(nullable: true),
                    UserUpdateId = table.Column<Guid>(nullable: true),
                    UserDeleteId = table.Column<Guid>(nullable: true),
                    DateInsert = table.Column<DateTime>(nullable: false),
                    DateUpdate = table.Column<DateTime>(nullable: true),
                    DateDelete = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagCategories_AspNetUsers_UserDeleteId",
                        column: x => x.UserDeleteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TagCategories_AspNetUsers_UserInsertId",
                        column: x => x.UserInsertId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TagCategories_AspNetUsers_UserUpdateId",
                        column: x => x.UserUpdateId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(nullable: true),
                    CategoryId = table.Column<int>(nullable: true),
                    UserInsertId = table.Column<Guid>(nullable: true),
                    UserUpdateId = table.Column<Guid>(nullable: true),
                    UserDeleteId = table.Column<Guid>(nullable: true),
                    DateInsert = table.Column<DateTime>(nullable: false),
                    DateUpdate = table.Column<DateTime>(nullable: true),
                    DateDelete = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_TagCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "TagCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tags_AspNetUsers_UserDeleteId",
                        column: x => x.UserDeleteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tags_AspNetUsers_UserInsertId",
                        column: x => x.UserInsertId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tags_AspNetUsers_UserUpdateId",
                        column: x => x.UserUpdateId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubjectTag",
                columns: table => new
                {
                    SubjectId = table.Column<int>(nullable: false),
                    TagId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectTag", x => new { x.SubjectId, x.TagId });
                    table.ForeignKey(
                        name: "FK_SubjectTag_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubjectTag_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubjectTag_TagId",
                table: "SubjectTag",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_TagCategories_UserDeleteId",
                table: "TagCategories",
                column: "UserDeleteId");

            migrationBuilder.CreateIndex(
                name: "IX_TagCategories_UserInsertId",
                table: "TagCategories",
                column: "UserInsertId");

            migrationBuilder.CreateIndex(
                name: "IX_TagCategories_UserUpdateId",
                table: "TagCategories",
                column: "UserUpdateId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_CategoryId",
                table: "Tags",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_UserDeleteId",
                table: "Tags",
                column: "UserDeleteId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_UserInsertId",
                table: "Tags",
                column: "UserInsertId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_UserUpdateId",
                table: "Tags",
                column: "UserUpdateId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubjectTag");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "TagCategories");
        }
    }
}
