using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MintPlayer.Data.Migrations
{
    public partial class AddCoreEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserInsertId = table.Column<Guid>(nullable: true),
                    UserUpdateId = table.Column<Guid>(nullable: true),
                    UserDeleteId = table.Column<Guid>(nullable: true),
                    SubjectType = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    YearStarted = table.Column<int>(nullable: true),
                    YearQuit = table.Column<int>(nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    Born = table.Column<DateTime>(nullable: true),
                    Died = table.Column<DateTime>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Released = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subjects_AspNetUsers_UserDeleteId",
                        column: x => x.UserDeleteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subjects_AspNetUsers_UserInsertId",
                        column: x => x.UserInsertId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subjects_AspNetUsers_UserUpdateId",
                        column: x => x.UserUpdateId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArtistPerson",
                columns: table => new
                {
                    ArtistId = table.Column<int>(nullable: false),
                    PersonId = table.Column<int>(nullable: false),
                    Active = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistPerson", x => new { x.ArtistId, x.PersonId });
                    table.ForeignKey(
                        name: "FK_ArtistPerson_Subjects_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArtistPerson_Subjects_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArtistSong",
                columns: table => new
                {
                    ArtistId = table.Column<int>(nullable: false),
                    SongId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistSong", x => new { x.ArtistId, x.SongId });
                    table.ForeignKey(
                        name: "FK_ArtistSong_Subjects_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArtistSong_Subjects_SongId",
                        column: x => x.SongId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Lyrics",
                columns: table => new
                {
                    SongId = table.Column<int>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    Text = table.Column<string>(nullable: true),
                    UpdatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lyrics", x => new { x.SongId, x.UserId });
                    table.ForeignKey(
                        name: "FK_Lyrics_Subjects_SongId",
                        column: x => x.SongId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lyrics_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArtistPerson_PersonId",
                table: "ArtistPerson",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistSong_SongId",
                table: "ArtistSong",
                column: "SongId");

            migrationBuilder.CreateIndex(
                name: "IX_Lyrics_UserId",
                table: "Lyrics",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_UserDeleteId",
                table: "Subjects",
                column: "UserDeleteId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_UserInsertId",
                table: "Subjects",
                column: "UserInsertId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_UserUpdateId",
                table: "Subjects",
                column: "UserUpdateId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtistPerson");

            migrationBuilder.DropTable(
                name: "ArtistSong");

            migrationBuilder.DropTable(
                name: "Lyrics");

            migrationBuilder.DropTable(
                name: "Subjects");
        }
    }
}
