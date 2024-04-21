using MintPlayer.Fetcher.Converters;
using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Musixmatch.Json;

internal class SongData
{
	[JsonProperty("page")]
	internal MxmPage Page { get; set; }
}

internal class MxmPage
{
	[JsonProperty("album")]
	public MxmAlbum Album { get; set; }
	[JsonProperty("tracks")]
	public MxmTrackCollection Tracks { get; set; }
}

internal class MxmAlbum
{
	public int Id { get; set; }
	public string Name { get; set; }
	[JsonConverter(typeof(MultiDateFormatConverter))]
	public DateTime ReleaseDate { get; set; }
	public int ArtistId { get; set; }
	[JsonProperty("coverart100x100")]
	public string CoverArt100 { get; set; }
	[JsonProperty("coverart350x350")]
	public string CoverArt350 { get; set; }
	[JsonProperty("coverart500x500")]
	public string CoverArt500 { get; set; }
	[JsonProperty("coverart800x800")]
	public string CoverArt800 { get; set; }
}

internal class MxmTrackCollection
{
	public List<MxmSong> List { get; set; }
	public MxmApiHeader ApiHeader { get; set; }
}

internal class MxmSong
{
	public int Id { get; set; }
	public string Name { get; set; }
	public int Length { get; set; }
	public int ArtistId { get; set; }
	public string ShareUrl { get; set; }
}

internal class MxmApiHeader
{
	public int StatusCode { get; set; }
	public int Available { get; set; }
}
