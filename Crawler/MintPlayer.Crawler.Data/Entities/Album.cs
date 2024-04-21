namespace MintPlayer.Crawler.Data.Entities;

internal class Album
{
	public int Id { get; set; }
	public Subject Subject { get; set; }

	public Artist Artist { get; set; }
	public List<Song> Tracks { get; set; }
}
