using System.Security.Cryptography;
using System.Text;
using MintPlayer.Migration.Target;
using MintPlayer.Migration.Transform;
using Newtonsoft.Json.Linq;

namespace MintPlayer.Migration.Tests;

public class PrimitivesTests
{
    private static readonly TimeZoneInfo Amsterdam = LegacyTime.Resolve("Europe/Amsterdam");
    private static readonly Guid User = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public void Ids_match_seed_catalog_and_spark_conventions()
    {
        Assert.Equal("Artists/12", Ids.Subject("artist", 12));
        Assert.Equal("People/3", Ids.Subject("person", 3));
        Assert.Equal("Songs/40", Ids.Subject("song", 40));
        Assert.Equal("MintPlayerUsers/11111111-1111-1111-1111-111111111111", Ids.User(User));
        Assert.Equal("UserLikes/MintPlayerUsers/11111111-1111-1111-1111-111111111111", Ids.UserLikes(Ids.User(User)));
        Assert.Throws<InvalidOperationException>(() => Ids.Subject("album", 1));
    }

    [Fact]
    public void Email_reservation_key_is_lowercased_normalized_email()
        => Assert.Equal("emails/someone@example.com", Ids.EmailReservation("SOMEONE@EXAMPLE.COM"));

    [Theory]
    [InlineData(null, false)]                                       // Subjects: NULL = live
    [InlineData("00000000-0000-0000-0000-000000000000", false)]    // Tags etc.: zero GUID = live
    [InlineData("22222222-2222-2222-2222-222222222222", false)]    // dangling user id = live (navigation is null)
    [InlineData("11111111-1111-1111-1111-111111111111", true)]     // joins a user = deleted
    public void Soft_delete_is_user_delete_id_joining_an_existing_user(string? userDeleteId, bool deleted)
        => Assert.Equal(deleted, SoftDelete.IsDeleted(userDeleteId is null ? null : Guid.Parse(userDeleteId), new HashSet<Guid> { User }));

    [Fact]
    public void Local_times_convert_to_utc_with_dst()
    {
        Assert.Equal(new DateTimeOffset(2021, 1, 15, 9, 0, 0, TimeSpan.Zero), LegacyTime.ToUtc(new DateTime(2021, 1, 15, 10, 0, 0), Amsterdam));
        Assert.Equal(new DateTimeOffset(2021, 7, 15, 8, 0, 0, TimeSpan.Zero), LegacyTime.ToUtc(new DateTime(2021, 7, 15, 10, 0, 0), Amsterdam));
        // spring-forward gap 02:30 does not exist → treated as 03:30 CEST
        Assert.Equal(new DateTimeOffset(2021, 3, 28, 1, 30, 0, TimeSpan.Zero), LegacyTime.ToUtc(new DateTime(2021, 3, 28, 2, 30, 0), Amsterdam));
        // fall-back 02:30 is ambiguous → first (CEST) occurrence
        Assert.Equal(new DateTimeOffset(2021, 10, 31, 0, 30, 0, TimeSpan.Zero), LegacyTime.ToUtc(new DateTime(2021, 10, 31, 2, 30, 0), Amsterdam));
        Assert.Null(LegacyTime.ToUtcOrNull(DateTime.MinValue, Amsterdam));
    }

    [Fact]
    public void Dates_drop_min_value_and_fix_utc_shifted_midnights()
    {
        Assert.Null(LegacyTime.ToDate(DateTime.MinValue, Amsterdam, out _));
        Assert.Null(LegacyTime.ToDate(null, Amsterdam, out _));
        Assert.Equal(new DateOnly(1980, 5, 3), LegacyTime.ToDate(new DateTime(1980, 5, 3), Amsterdam, out var s1));
        Assert.False(s1);
        // 22:00 UTC in summer = local midnight of the next day
        Assert.Equal(new DateOnly(1980, 5, 4), LegacyTime.ToDate(new DateTime(1980, 5, 3, 22, 0, 0), Amsterdam, out var s2));
        Assert.True(s2);
    }

    [Fact]
    public void Colors_drop_alpha_and_report_it()
    {
        var c = Colors.FromLegacy(unchecked((int)0x80FF8800), out var alpha);
        Assert.True(alpha);
        Assert.Equal((255, 255, 136, 0), (c.A, c.R, c.G, c.B));
        Colors.FromLegacy(unchecked((int)0xFF00B900), out var opaque);
        Assert.False(opaque);
    }

    [Fact]
    public void Spark_color_converter_writes_rrggbb()
    {
        var json = JObject.FromObject(new MintPlayer.Domain.Entities.TagCategory { Color = System.Drawing.Color.FromArgb(255, 0, 185, 0) },
            SparkConventions.CreateComparisonSerializer());
        Assert.Equal("#00b900", (string?)json["Color"]);
    }

    [Fact]
    public void Recovery_code_hash_matches_spark_master_user_store()
    {
        // UserStore.HashRecoveryCode: Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(code)))
        var expected = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("ABCDE-12345"))).ToLowerInvariant();
        Assert.Equal(expected, RecoveryCodes.Hash("ABCDE-12345"));
        Assert.Equal(64, expected.Length);
        Assert.Equal(["a", "b", "c"], RecoveryCodes.Split("a;b;;c"));
        Assert.Empty(RecoveryCodes.Split(null));
    }

    [Fact]
    public void Referenced_spark_preview41_stores_recovery_codes_verbatim()
        => Assert.False(SparkConventions.UserStoreHashesRecoveryCodes()); // flips when Spark is upgraded past preview.41

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=abc123", true)]
    [InlineData("https://youtu.be/abc123", true)]
    [InlineData("https://vimeo.com/123456", true)]
    [InlineData("https://www.dailymotion.com/video/x7abc", true)]
    [InlineData("https://soundcloud.com/artist/track", true)]
    [InlineData("https://open.spotify.com/track/4uLU6hMCjMI75M1A2tKUQC", true)]
    [InlineData("https://example.com/file.mp3", true)]
    [InlineData("https://en.wikipedia.org/wiki/Queen", false)]
    [InlineData("https://genius.com/Some-lyrics", false)]
    [InlineData("https://vimeo.com/channels/staff", false)]
    [InlineData("", false)]
    public void Playability_mirrors_the_video_player_plugins(string url, bool playable)
        => Assert.Equal(playable, Playability.IsPlayable(url));

    [Fact]
    public void First_playable_follows_media_order()
        => Assert.Equal("https://youtu.be/x", Playability.FirstPlayable(["https://en.wikipedia.org/wiki/X", "https://youtu.be/x", "https://vimeo.com/1"]));
}
