namespace MintPlayer.Fetcher.SongtekstenNet.Data;

internal class Song
{
	public int Id { get; init; }
	public string Title { get; init; }
	public string Url { get; init; }

	public MintPlayer.Fetcher.Abstractions.Dtos.Song ToDto()
	{
		return new MintPlayer.Fetcher.Abstractions.Dtos.Song
		{
			Id = Id,
			Title = Title,
			Url = Url
		};
	}
}
