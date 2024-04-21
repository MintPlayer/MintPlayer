namespace MintPlayer.Fetcher.Integration.Dtos;

public class FetchedArtist : FetchedSubject
{
	public string Url { get; set; }
	public string Name { get; set; }
	public string ImageUrl { get; set; }

	public List<FetchResult<FetchedSong>> Songs { get; set; }
	public List<FetchResult<FetchedAlbum>> Albums { get; set; }
}
