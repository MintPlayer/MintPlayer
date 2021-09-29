namespace MintPlayer.Fetcher.Abstractions
{
	/// <summary>Wrapper used to look for an existing subject in the database.</summary>
	public class SubjectLookup
	{
		/// <summary>Url of the fetched subject.</summary>
		public string Url { get; set; }
		/// <summary>Keyword for the fetched subject (Artist name, song title, album title).</summary>
		public string Keyword { get; set; }
		/// <summary>Types to look for.</summary>
		public string[] SubjectTypes { get; set; }
	}
}
