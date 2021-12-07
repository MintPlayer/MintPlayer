using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MintPlayer.Data.Migrations
{
    public partial class UseTptMapping : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "FK_Lyrics_Subjects_SongId",
                table: "Lyrics");

            migrationBuilder.DropForeignKey(
                name: "FK_PlaylistSong_Subjects_SongId",
                table: "PlaylistSong");

			migrationBuilder.CreateTable(
				name: "Artists",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false),
					Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
					YearStarted = table.Column<int>(type: "int", nullable: true),
					YearQuit = table.Column<int>(type: "int", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Artists", x => x.Id);
					table.ForeignKey(
						name: "FK_Artists_Subjects_Id",
						column: x => x.Id,
						principalTable: "Subjects",
						principalColumn: "Id");
				});

			migrationBuilder.CreateTable(
				name: "People",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false),
					FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					Born = table.Column<DateTime>(type: "datetime2", nullable: true),
					Died = table.Column<DateTime>(type: "datetime2", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_People", x => x.Id);
					table.ForeignKey(
						name: "FK_People_Subjects_Id",
						column: x => x.Id,
						principalTable: "Subjects",
						principalColumn: "Id");
				});

			migrationBuilder.CreateTable(
				name: "Songs",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false),
					Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
					Released = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Songs", x => x.Id);
					table.ForeignKey(
						name: "FK_Songs_Subjects_Id",
						column: x => x.Id,
						principalTable: "Subjects",
						principalColumn: "Id");
				});

			migrationBuilder.Sql(@"INSERT INTO [People] (Id, FirstName, LastName, Born, Died)
				SELECT Id, FirstName, LastName, Born, Died
				FROM [Subjects]
				WHERE [Subjects].[SubjectType] = 'person'");

			migrationBuilder.Sql(@"INSERT INTO [Artists] (Id, Name, YearStarted, YearQuit)
				SELECT Id, Name, YearStarted, YearQuit
				FROM [Subjects]
				WHERE [Subjects].[SubjectType] = 'artist'");

			migrationBuilder.Sql(@"INSERT INTO [Songs] (Id, Title, Released)
				SELECT Id, Title, Released
				FROM [Subjects]
				WHERE [Subjects].[SubjectType] = 'song'");

			migrationBuilder.DropColumn(
                name: "Born",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "Died",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "Released",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "SubjectType",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "YearQuit",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "YearStarted",
                table: "Subjects");

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistPerson_Artists_ArtistId",
                table: "ArtistPerson",
                column: "ArtistId",
                principalTable: "Artists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistPerson_People_PersonId",
                table: "ArtistPerson",
                column: "PersonId",
                principalTable: "People",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistSong_Artists_ArtistId",
                table: "ArtistSong",
                column: "ArtistId",
                principalTable: "Artists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistSong_Songs_SongId",
                table: "ArtistSong",
                column: "SongId",
                principalTable: "Songs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lyrics_Songs_SongId",
                table: "Lyrics",
                column: "SongId",
                principalTable: "Songs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlaylistSong_Songs_SongId",
                table: "PlaylistSong",
                column: "SongId",
                principalTable: "Songs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArtistPerson_Artists_ArtistId",
                table: "ArtistPerson");

            migrationBuilder.DropForeignKey(
                name: "FK_ArtistPerson_People_PersonId",
                table: "ArtistPerson");

            migrationBuilder.DropForeignKey(
                name: "FK_ArtistSong_Artists_ArtistId",
                table: "ArtistSong");

            migrationBuilder.DropForeignKey(
                name: "FK_ArtistSong_Songs_SongId",
                table: "ArtistSong");

            migrationBuilder.DropForeignKey(
                name: "FK_Lyrics_Songs_SongId",
                table: "Lyrics");

            migrationBuilder.DropForeignKey(
                name: "FK_PlaylistSong_Songs_SongId",
                table: "PlaylistSong");

            migrationBuilder.DropTable(
                name: "Artists");

            migrationBuilder.DropTable(
                name: "People");

            migrationBuilder.DropTable(
                name: "Songs");

            migrationBuilder.AddColumn<DateTime>(
                name: "Born",
                table: "Subjects",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Died",
                table: "Subjects",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Subjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Subjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Subjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Released",
                table: "Subjects",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubjectType",
                table: "Subjects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Subjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearQuit",
                table: "Subjects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearStarted",
                table: "Subjects",
                type: "int",
                nullable: true);

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
                name: "FK_Lyrics_Subjects_SongId",
                table: "Lyrics",
                column: "SongId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlaylistSong_Subjects_SongId",
                table: "PlaylistSong",
                column: "SongId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
