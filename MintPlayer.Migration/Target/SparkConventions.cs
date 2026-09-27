using System.Reflection;
using MintPlayer.Spark.Authorization.Identity;
using Newtonsoft.Json;
using Raven.Client.Documents;
using Raven.Client.Json.Serialization.NewtonsoftJson;

namespace MintPlayer.Migration.Target;

/// <summary>
/// Reproduces the <c>DocumentStore</c> that <c>AddSpark</c> builds for MintPlayer.Web (decompiled from
/// MintPlayer.Spark 10.0.0-preview.41 <c>SparkExtensions</c>): Newtonsoft serialization with Spark's own
/// <c>ColorNewtonsoftJsonConverter</c> (internal, so it is instantiated by reflection from the referenced
/// Spark assembly — the migration writes colours exactly as the app reads them) and Spark's
/// <c>{collection}/{Guid}</c> id generator (unused here: every id is explicit).
/// </summary>
public static class SparkConventions
{
    private const string ColorConverterTypeName = "MintPlayer.Spark.Converters.ColorNewtonsoftJsonConverter";

    public static JsonConverter CreateColorConverter()
    {
        var sparkAssembly = typeof(MintPlayer.Spark.SparkContext).Assembly;
        var type = sparkAssembly.GetType(ColorConverterTypeName, throwOnError: false)
            ?? throw new InvalidOperationException($"{ColorConverterTypeName} not found in {sparkAssembly.GetName()} — Spark changed its colour serialisation; update SparkConventions.");
        return (JsonConverter)Activator.CreateInstance(type, nonPublic: true)!;
    }

    public static DocumentStore CreateStore(string url, string database)
    {
        var store = new DocumentStore { Urls = [url], Database = database };
        store.Conventions.AsyncDocumentIdGenerator = (_, entity) =>
            Task.FromResult($"{store.Conventions.GetCollectionName(entity.GetType())}/{Guid.NewGuid()}");
        store.Conventions.Serialization = new NewtonsoftJsonSerializationConventions
        {
            CustomizeJsonSerializer = serializer => serializer.Converters.Add(CreateColorConverter()),
        };
        store.Initialize();
        return store;
    }

    /// <summary>Plain Newtonsoft serializer with the Spark colour converter — used for the dry-run JSONL and
    /// for the reconciler's round-trip comparison (both sides go through the same serializer).</summary>
    public static JsonSerializer CreateComparisonSerializer()
    {
        var serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
        {
            DateParseHandling = DateParseHandling.None,
            NullValueHandling = NullValueHandling.Include,
        });
        serializer.Converters.Add(CreateColorConverter());
        return serializer;
    }

    /// <summary>
    /// Whether the referenced Spark <c>UserStore</c> hashes recovery codes (<c>HashRecoveryCode</c>, Spark master)
    /// or stores them verbatim (≤ preview.41). The stored form must match what the running app compares against.
    /// </summary>
    public static bool UserStoreHashesRecoveryCodes()
        => typeof(UserStore<>).GetMethod("HashRecoveryCode", BindingFlags.NonPublic | BindingFlags.Static) != null;

    public static string SparkVersion()
        => typeof(UserStore<>).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
           ?? typeof(UserStore<>).Assembly.GetName().Version?.ToString() ?? "?";
}
