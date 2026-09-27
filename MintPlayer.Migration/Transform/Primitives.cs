using System.Drawing;
using System.Security.Cryptography;
using System.Text;

namespace MintPlayer.Migration.Transform;

/// <summary>Deterministic document ids (§5.2) — identical to <c>scripts/seed-catalog.mjs</c>.</summary>
public static class Ids
{
    public static string Artist(int id) => $"Artists/{id}";
    public static string Person(int id) => $"People/{id}";
    public static string Song(int id) => $"Songs/{id}";
    public static string Tag(int id) => $"Tags/{id}";
    public static string TagCategory(int id) => $"TagCategories/{id}";
    public static string MediumType(int id) => $"MediumTypes/{id}";
    public static string Playlist(int id) => $"Playlists/{id}";
    public static string BlogPost(int id) => $"BlogPosts/{id}";

    /// <summary>Spark's own id generator makes <c>{collection}/{Guid}</c> ("D" format); legacy ids are
    /// GUIDs already, so users keep their identity: <c>MintPlayerUsers/{guid:D}</c>.</summary>
    public static string User(Guid id) => $"MintPlayerUsers/{id:D}";

    /// <summary><c>UserLikes/{userId}</c> — as <c>SubjectController.DocId</c>.</summary>
    public static string UserLikes(string userId) => $"UserLikes/{userId}";

    /// <summary>Id of a subject by its TPH discriminator; throws on an unknown discriminator (§5.2 "fail").</summary>
    public static string Subject(string subjectType, int id) => subjectType switch
    {
        "artist" => Artist(id),
        "person" => Person(id),
        "song" => Song(id),
        _ => throw new InvalidOperationException($"Unknown subject discriminator '{subjectType}' on subject {id}."),
    };

    /// <summary>Compare-exchange key Spark's <c>UserStore</c> uses for email lookup/uniqueness:
    /// <c>"emails/" + normalizedEmail.ToLowerInvariant()</c> (same in preview.41 and master).</summary>
    public static string EmailReservation(string normalizedEmail) => "emails/" + normalizedEmail.ToLowerInvariant();
}

/// <summary>
/// Legacy soft delete (S2): the EF query filter is <c>UserDelete == null</c> on the <em>navigation</em>, so a
/// row is deleted only when <c>UserDeleteId</c> joins an existing user. The zero GUID (live rows in
/// MediumTypes/Tags/TagCategories/BlogPosts) and a bare <c>DateDelete</c> are live.
/// </summary>
public static class SoftDelete
{
    public static bool IsDeleted(Guid? userDeleteId, IReadOnlySet<Guid> existingUserIds)
        => userDeleteId is { } id && id != Guid.Empty && existingUserIds.Contains(id);
}

/// <summary>Legacy <c>DateTime.Now</c> values are Europe/Amsterdam wall-clock times (D19).</summary>
public static class LegacyTime
{
    public const string DefaultTimeZoneId = "Europe/Amsterdam";

    public static TimeZoneInfo Resolve(string timeZoneId) => TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

    public static bool IsMin(DateTime value) => value < new DateTime(1, 1, 2);

    /// <summary>
    /// Converts a legacy local wall-clock time to a UTC instant. Times inside the spring-forward gap are
    /// shifted forward by the gap (the wall clock the server would have shown); ambiguous times in the
    /// autumn fall-back hour resolve to the first (daylight-saving) occurrence.
    /// </summary>
    public static DateTimeOffset ToUtc(DateTime local, TimeZoneInfo zone)
    {
        var unspecified = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
        if (zone.IsInvalidTime(unspecified))
            unspecified = unspecified.AddHours(1);

        TimeSpan offset;
        if (zone.IsAmbiguousTime(unspecified))
            offset = zone.GetAmbiguousTimeOffsets(unspecified).Max();
        else
            offset = zone.GetUtcOffset(unspecified);

        return new DateTimeOffset(unspecified, offset).ToUniversalTime();
    }

    public static DateTimeOffset? ToUtcOrNull(DateTime? local, TimeZoneInfo zone)
        => local is { } v && !IsMin(v) ? ToUtc(v, zone) : null;

    /// <summary>
    /// A legacy calendar date (Born/Died/Released). <c>0001-01-01</c> → null. A value with a time part is
    /// a local midnight that a browser serialised as UTC (S5: two Born values at 22:00) — convert that
    /// instant to the legacy zone and take its date, so it does not land one day early.
    /// </summary>
    public static DateOnly? ToDate(DateTime? value, TimeZoneInfo zone, out bool shifted)
    {
        shifted = false;
        if (value is not { } v || IsMin(v))
            return null;
        if (v.TimeOfDay == TimeSpan.Zero)
            return DateOnly.FromDateTime(v);

        shifted = true;
        var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(v, DateTimeKind.Utc), zone);
        return DateOnly.FromDateTime(local);
    }
}

public static class Colors
{
    /// <summary>Legacy stores <c>Color.ToArgb()</c>. Spark's colour converter persists #rrggbb, so alpha is
    /// lost by design (§5.2) — callers report alpha ≠ 255.</summary>
    public static Color FromLegacy(int argb, out bool hadAlpha)
    {
        var color = Color.FromArgb(argb);
        hadAlpha = color.A != 255;
        return Color.FromArgb(255, color.R, color.G, color.B);
    }
}

public static class RecoveryCodes
{
    /// <summary>Spark master <c>UserStore.HashRecoveryCode</c>: SHA-256 over UTF-8, lower-case hex.</summary>
    public static string Hash(string code) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(code)));

    /// <summary>Identity's token store keeps codes as one <c>;</c>-joined string.</summary>
    public static IReadOnlyList<string> Split(string? stored)
        => string.IsNullOrEmpty(stored) ? [] : stored.Split(';', StringSplitOptions.RemoveEmptyEntries);
}
