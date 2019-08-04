using Microsoft.AspNetCore.Identity;

namespace MintPlayer.Data.Entities
{
    internal class User : IdentityUser<int>
    {
        public string PictureUrl { get; set; }
    }
}
