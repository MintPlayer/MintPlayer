using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MintPlayer.Data.Migrations
{
    public partial class AddMedia : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArtistPerson_Subject_ArtistId",
                table: "ArtistPerson");

            migrationBuilder.DropForeignKey(
                name: "FK_ArtistPerson_Subject_PersonId",
                table: "ArtistPerson");

            migrationBuilder.DropForeignKey(
                name: "FK_ArtistSong_Subject_ArtistId",
                table: "ArtistSong");

            migrationBuilder.DropForeignKey(
                name: "FK_ArtistSong_Subject_SongId",
                table: "ArtistSong");

            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Subject_SubjectId",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_Lyrics_Subject_SongId",
                table: "Lyrics");

            migrationBuilder.DropForeignKey(
                name: "FK_Subject_AspNetUsers_UserDeleteId",
                table: "Subject");

            migrationBuilder.DropForeignKey(
                name: "FK_Subject_AspNetUsers_UserInsertId",
                table: "Subject");

            migrationBuilder.DropForeignKey(
                name: "FK_Subject_AspNetUsers_UserUpdateId",
                table: "Subject");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Subject",
                table: "Subject");

            migrationBuilder.RenameTable(
                name: "Subject",
                newName: "Subjects");

            migrationBuilder.RenameIndex(
                name: "IX_Subject_UserUpdateId",
                table: "Subjects",
                newName: "IX_Subjects_UserUpdateId");

            migrationBuilder.RenameIndex(
                name: "IX_Subject_UserInsertId",
                table: "Subjects",
                newName: "IX_Subjects_UserInsertId");

            migrationBuilder.RenameIndex(
                name: "IX_Subject_UserDeleteId",
                table: "Subjects",
                newName: "IX_Subjects_UserDeleteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subjects",
                table: "Subjects",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Media",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TypeId = table.Column<int>(nullable: true),
                    SubjectId = table.Column<int>(nullable: true),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Media", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Media_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Media_MediumTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "MediumTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Media_SubjectId",
                table: "Media",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Media_TypeId",
                table: "Media",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistPerson_Subjects_ArtistId",
                table: "ArtistPerson",
                column: "ArtistId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistPerson_Subjects_PersonId",
                table: "ArtistPerson",
                column: "PersonId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistSong_Subjects_ArtistId",
                table: "ArtistSong",
                column: "ArtistId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistSong_Subjects_SongId",
                table: "ArtistSong",
                column: "SongId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Subjects_SubjectId",
                table: "Likes",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lyrics_Subjects_SongId",
                table: "Lyrics",
                column: "SongId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_AspNetUsers_UserDeleteId",
                table: "Subjects",
                column: "UserDeleteId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_AspNetUsers_UserInsertId",
                table: "Subjects",
                column: "UserInsertId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_AspNetUsers_UserUpdateId",
                table: "Subjects",
                column: "UserUpdateId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArtistPerson_Subjects_ArtistId",
                table: "ArtistPerson");

            migrationBuilder.DropForeignKey(
                name: "FK_ArtistPerson_Subjects_PersonId",
                table: "ArtistPerson");

            migrationBuilder.DropForeignKey(
                name: "FK_ArtistSong_Subjects_ArtistId",
                table: "ArtistSong");

            migrationBuilder.DropForeignKey(
                name: "FK_ArtistSong_Subjects_SongId",
                table: "ArtistSong");

            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Subjects_SubjectId",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_Lyrics_Subjects_SongId",
                table: "Lyrics");

            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_AspNetUsers_UserDeleteId",
                table: "Subjects");

            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_AspNetUsers_UserInsertId",
                table: "Subjects");

            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_AspNetUsers_UserUpdateId",
                table: "Subjects");

            migrationBuilder.DropTable(
                name: "Media");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Subjects",
                table: "Subjects");

            migrationBuilder.RenameTable(
                name: "Subjects",
                newName: "Subject");

            migrationBuilder.RenameIndex(
                name: "IX_Subjects_UserUpdateId",
                table: "Subject",
                newName: "IX_Subject_UserUpdateId");

            migrationBuilder.RenameIndex(
                name: "IX_Subjects_UserInsertId",
                table: "Subject",
                newName: "IX_Subject_UserInsertId");

            migrationBuilder.RenameIndex(
                name: "IX_Subjects_UserDeleteId",
                table: "Subject",
                newName: "IX_Subject_UserDeleteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subject",
                table: "Subject",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistPerson_Subject_ArtistId",
                table: "ArtistPerson",
                column: "ArtistId",
                principalTable: "Subject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistPerson_Subject_PersonId",
                table: "ArtistPerson",
                column: "PersonId",
                principalTable: "Subject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistSong_Subject_ArtistId",
                table: "ArtistSong",
                column: "ArtistId",
                principalTable: "Subject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistSong_Subject_SongId",
                table: "ArtistSong",
                column: "SongId",
                principalTable: "Subject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Subject_SubjectId",
                table: "Likes",
                column: "SubjectId",
                principalTable: "Subject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lyrics_Subject_SongId",
                table: "Lyrics",
                column: "SongId",
                principalTable: "Subject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Subject_AspNetUsers_UserDeleteId",
                table: "Subject",
                column: "UserDeleteId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Subject_AspNetUsers_UserInsertId",
                table: "Subject",
                column: "UserInsertId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Subject_AspNetUsers_UserUpdateId",
                table: "Subject",
                column: "UserUpdateId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
