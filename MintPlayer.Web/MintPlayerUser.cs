using MintPlayer.Spark.Authorization.Identity;

namespace MintPlayer.Web;

/// <summary>
/// The MintPlayer identity user. Extends Spark's <see cref="SparkUser"/> with the two
/// MintPlayer-specific fields carried over from the legacy app's ApplicationUser.
/// </summary>
public class MintPlayerUser : SparkUser
{
    /// <summary>Avatar URL (often sourced from a linked social login).</summary>
    public string? PictureUrl { get; set; }

    /// <summary>When true, an external (social) login skips the 2FA challenge.</summary>
    public bool Bypass2faForExternalLogin { get; set; }
}
