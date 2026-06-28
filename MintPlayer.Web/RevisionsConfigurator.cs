using Raven.Client.Documents;
using Raven.Client.Documents.Operations.Revisions;

namespace MintPlayer.Web;

/// <summary>
/// Enables RavenDB document revisions for the collections that need an edit history. Today that is
/// <c>Songs</c>: lyrics are embedded on the song and can be edited by any signed-in user (through the
/// lyrics API), so versioning the song document makes every lyric change recoverable.
///
/// <para>Runs at startup against the <see cref="IDocumentStore"/> that <c>AddSpark</c> registers — Spark
/// exposes no per-collection revisions hook, but <see cref="ConfigureRevisionsOperation"/> is a public
/// maintenance API, so no framework change is needed. It must run <b>after</b> <c>UseSpark</c> (which exits
/// the process during <c>--spark-synchronize-model</c>) and is idempotent — applying the same configuration
/// on every boot is a no-op.</para>
/// </summary>
internal static class RevisionsConfigurator
{
    public static async Task ConfigureRevisionsAsync(this WebApplication app)
    {
        var store = app.Services.GetRequiredService<IDocumentStore>();

        await store.Maintenance.SendAsync(new ConfigureRevisionsOperation(new RevisionsConfiguration
        {
            Collections = new Dictionary<string, RevisionsCollectionConfiguration>
            {
                ["Songs"] = new RevisionsCollectionConfiguration { Disabled = false },
            },
        }));
    }
}
