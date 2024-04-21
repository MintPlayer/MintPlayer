namespace MintPlayer.Fetcher.SongtekstenNet.Data;

internal class Artist
{
	public int Id { get; init; }
	public string Name { get; init; }
	public string Url { get; init; }

	public MintPlayer.Fetcher.Abstractions.Dtos.Artist ToDto()
	{
		return new MintPlayer.Fetcher.Abstractions.Dtos.Artist
		{
			Id = Id,
			Name = Name,
			Url = Url
		};
	}
}
