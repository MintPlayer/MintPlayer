namespace MintPlayer.Fetcher.Genius.Parsers.V1.Mappers;

internal class ArtistV1Mapper
{
	public async Task<MintPlayer.Fetcher.Abstractions.Dtos.Artist> ToDto(Data.ArtistData artist)
	{
		if (artist == null)
		{
			return null;
		}

		var result = new MintPlayer.Fetcher.Abstractions.Dtos.Artist
		{
			Id = artist.Id,
			Url = artist.Url,
			Name = artist.Name,
		};

		return result;
	}
}
