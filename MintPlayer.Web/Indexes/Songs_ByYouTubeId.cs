using System.Text.RegularExpressions;
using MintPlayer.Domain.Entities;
using MintPlayer.Web.YouTube;
using Raven.Client.Documents.Indexes;

namespace MintPlayer.Web.Indexes;

/// <summary>
/// Songs by the canonical YouTube video id of each of their media URLs — the lookup behind the YouTube
/// playlist import (F16). The catalog stores many URL variants (<c>www.</c>/<c>m.youtube.com/watch?v=</c>,
/// <c>youtu.be/</c>, trailing <c>&amp;ab_channel=</c>, …); the map normalises them with the same
/// <see cref="YouTubeUrl.VideoIdPattern"/> the importer uses, so a playlist of N videos is matched with one
/// <c>where YouTubeIds in (…)</c> query instead of scanning every song. Soft-deleted songs are not indexed.
/// Auto-registered at startup (scanned from this assembly).
/// </summary>
public class Songs_ByYouTubeId : AbstractIndexCreationTask<Song, Songs_ByYouTubeId.Result>
{
    public class Result
    {
        public string[] YouTubeIds { get; set; } = [];
    }

    public Songs_ByYouTubeId()
    {
        Map = songs => from song in songs
                       where song.IsDeleted == false
                       select new Result
                       {
                           YouTubeIds = song.Media
                               .Where(m => m.Value != null)
                               .Select(m => Regex.Match(m.Value, YouTubeUrl.VideoIdPattern))
                               .Where(match => match.Success)
                               .Select(match => match.Groups[1].Value)
                               .ToArray(),
                       };
    }
}
