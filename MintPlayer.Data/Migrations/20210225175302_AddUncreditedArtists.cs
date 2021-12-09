using Microsoft.EntityFrameworkCore.Migrations;

namespace MintPlayer.Data.Migrations
{
    public partial class AddUncreditedArtists : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Credited",
                table: "ArtistSong",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Credited",
                table: "ArtistSong");
        }
    }
}
