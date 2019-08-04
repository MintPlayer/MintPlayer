using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace MintPlayer.Data.Entities
{
    internal class User : IdentityUser<int>
    {
        public string PictureUrl { get; set; }
        public List<Lyrics> Lyrics { get; set; }
    }
}
