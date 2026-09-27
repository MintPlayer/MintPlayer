namespace MintPlayer.Migration.Transform;

public enum IssueSeverity { Info, Warning, Error }

/// <summary>One finding of the transform or the reconciler. <see cref="Subject"/> is always an id or a
/// table name — never a personal value (no emails, names, hashes, tokens).</summary>
public sealed record MigrationIssue(IssueSeverity Severity, string Code, string Subject, string Detail)
{
    public override string ToString() => $"{Severity,-7} {Code,-28} {Subject,-48} {Detail}";
}

/// <summary>A document to store with an explicit id.</summary>
public sealed record TargetDocument(string Id, string Collection, object Entity);

/// <summary>Everything the transform produced; the writers only persist it.</summary>
public sealed class MigrationPlan
{
    public List<TargetDocument> Documents { get; } = [];
    /// <summary>Compare-exchange key → value (<c>emails/{normalizedEmail}</c> → user id).</summary>
    public SortedDictionary<string, string> CompareExchange { get; } = new(StringComparer.Ordinal);
    public List<MigrationIssue> Issues { get; } = [];

    public void Add(string id, string collection, object entity) => Documents.Add(new TargetDocument(id, collection, entity));

    public void Issue(IssueSeverity severity, string code, string subject, string detail = "")
        => Issues.Add(new MigrationIssue(severity, code, subject, detail));

    public IEnumerable<T> Of<T>() => Documents.Select(d => d.Entity).OfType<T>();
}

public sealed record TransformOptions
{
    public required TimeZoneInfo TimeZone { get; init; }
    /// <summary>Fallback CreatedAt for rows without a usable insert date (and tables without audit columns).</summary>
    public required DateTimeOffset MigratedAt { get; init; }
    /// <summary>Hash recovery codes (Spark master) or store them verbatim (Spark ≤ preview.41).</summary>
    public required bool HashRecoveryCodes { get; init; }
    /// <summary>Group names from <c>App_Data/security.json</c>; a legacy role must match one.</summary>
    public required IReadOnlySet<string> KnownGroups { get; init; }
}
