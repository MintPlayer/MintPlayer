using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Identity;

[assembly: InternalsVisibleTo("MintPlayerCrawler.Data")]
namespace MintPlayer.Data.Entities
{
    internal class User : IdentityUser<int>
    {
        public string PictureUrl { get; set; }
        public List<Lyrics> Lyrics { get; set; }
        public List<Like> Likes { get; set; }
    }
}
