using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MintPlayer.Data.Migrations
{
    public partial class NullableUserDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "mintplay");

            migrationBuilder.RenameTable(
                name: "Tags",
                newName: "Tags",
                newSchema: "mintplay");

            migrationBuilder.RenameTable(
                name: "TagCategories",
                newName: "TagCategories",
                newSchema: "mintplay");

            migrationBuilder.RenameTable(
                name: "SubjectTag",
                newName: "SubjectTag",
                newSchema: "mintplay");

            migrationBuilder.RenameTable(
                name: "Subjects",
                newName: "Subjects",
                newSchema: "mintplay");

            migrationBuilder.RenameTable(
                name: "Songs",
                newName: "Songs",
                newSchema: "mintplay");

            migrationBuilder.RenameTable(
                name: "PlaylistSong",
                newName: "PlaylistSong",
                newSchema: "mintplay");

            migrationBuilder.RenameTable(
                name: "Playlists",
                newName: "Playlists",
                newSchema: "mintplay");

            migrationBuilder.RenameTable(
                name: "People",
                newName: "People",
                newSchema: "mintplay");

            migrationBuilder.RenameTable(
                name: "MediumTypes",
                newName: "MediumTypes",
                newSchema: "mintplay");

            migrationBuilder.RenameTable(
                name: "Media",
                newName: "Media",
                newSchema: "mintplay");

            migrationBuilder.RenameTable(
                name: "Lyrics",
                newName: "Lyrics",
                newSchema: "mintplay");

            migrationBuilder.RenameTable(
                name: "LogEntries",
                newName: "LogEntries",
                newSchema: "mintplay");

            migrationBuilder.RenameTable(
                name: "Likes",
                newName: "Likes",
                newSchema: "mintplay");

            migrationBuilder.RenameTable(
                name: "Jobs",
                newName: "Jobs",
                newSchema: "mintplay");

            migrationBuilder.RenameTable(
                name: "BlogPosts",
                newName: "BlogPosts",
                newSchema: "mintplay");

            migrationBuilder.RenameTable(
                name: "ArtistSong",
                newName: "ArtistSong",
                newSchema: "mintplay");

            migrationBuilder.RenameTable(
                name: "Artists",
                newName: "Artists",
                newSchema: "mintplay");

            migrationBuilder.RenameTable(
                name: "ArtistPerson",
                newName: "ArtistPerson",
                newSchema: "mintplay");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Tags",
                schema: "mintplay",
                newName: "Tags");

            migrationBuilder.RenameTable(
                name: "TagCategories",
                schema: "mintplay",
                newName: "TagCategories");

            migrationBuilder.RenameTable(
                name: "SubjectTag",
                schema: "mintplay",
                newName: "SubjectTag");

            migrationBuilder.RenameTable(
                name: "Subjects",
                schema: "mintplay",
                newName: "Subjects");

            migrationBuilder.RenameTable(
                name: "Songs",
                schema: "mintplay",
                newName: "Songs");

            migrationBuilder.RenameTable(
                name: "PlaylistSong",
                schema: "mintplay",
                newName: "PlaylistSong");

            migrationBuilder.RenameTable(
                name: "Playlists",
                schema: "mintplay",
                newName: "Playlists");

            migrationBuilder.RenameTable(
                name: "People",
                schema: "mintplay",
                newName: "People");

            migrationBuilder.RenameTable(
                name: "MediumTypes",
                schema: "mintplay",
                newName: "MediumTypes");

            migrationBuilder.RenameTable(
                name: "Media",
                schema: "mintplay",
                newName: "Media");

            migrationBuilder.RenameTable(
                name: "Lyrics",
                schema: "mintplay",
                newName: "Lyrics");

            migrationBuilder.RenameTable(
                name: "LogEntries",
                schema: "mintplay",
                newName: "LogEntries");

            migrationBuilder.RenameTable(
                name: "Likes",
                schema: "mintplay",
                newName: "Likes");

            migrationBuilder.RenameTable(
                name: "Jobs",
                schema: "mintplay",
                newName: "Jobs");

            migrationBuilder.RenameTable(
                name: "BlogPosts",
                schema: "mintplay",
                newName: "BlogPosts");

            migrationBuilder.RenameTable(
                name: "ArtistSong",
                schema: "mintplay",
                newName: "ArtistSong");

            migrationBuilder.RenameTable(
                name: "Artists",
                schema: "mintplay",
                newName: "Artists");

            migrationBuilder.RenameTable(
                name: "ArtistPerson",
                schema: "mintplay",
                newName: "ArtistPerson");
        }
    }
}
