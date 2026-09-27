using System.Diagnostics;
using MintPlayer.Migration.Transform;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Session;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

namespace MintPlayer.Migration.Target;

/// <summary>
/// Writes a <see cref="MigrationPlan"/> into a freshly (re)created database: BulkInsert with explicit ids,
/// compare-exchange email reservations, then the app's indexes (from the MintPlayer.Web assembly, as
/// Spark's <c>CreateSparkIndexes</c> does at startup) and a wait until they are non-stale.
/// </summary>
public sealed class RavenWriter(IDocumentStore store, TextWriter log)
{
    public async Task RecreateDatabaseAsync(CancellationToken ct = default)
    {
        var name = store.Database;
        var existing = await store.Maintenance.Server.SendAsync(new GetDatabaseNamesOperation(0, int.MaxValue), ct);
        if (existing.Contains(name, StringComparer.OrdinalIgnoreCase))
        {
            log.WriteLine($"  deleting existing database '{name}'");
            await store.Maintenance.Server.SendAsync(new DeleteDatabasesOperation(name, hardDelete: true, timeToWaitForConfirmation: TimeSpan.FromSeconds(30)), ct);
        }
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                await store.Maintenance.Server.SendAsync(new CreateDatabaseOperation(new DatabaseRecord(name)), ct);
                break;
            }
            catch (Raven.Client.Exceptions.RavenException) when (attempt < 20)
            {
                await Task.Delay(250, ct); // delete still settling
            }
        }
        log.WriteLine($"  created database '{name}'");
    }

    public async Task WriteDocumentsAsync(MigrationPlan plan, CancellationToken ct = default)
    {
        foreach (var doc in plan.Documents)
        {
            var conventional = store.Conventions.GetCollectionName(doc.Entity.GetType());
            if (!string.Equals(conventional, doc.Collection, StringComparison.Ordinal))
                throw new InvalidOperationException($"{doc.Id}: plan collection '{doc.Collection}' ≠ convention '{conventional}'.");
            if (!doc.Id.StartsWith(doc.Collection + "/", StringComparison.Ordinal))
                throw new InvalidOperationException($"{doc.Id}: id does not start with its collection '{doc.Collection}/'.");
        }

        await using (var bulk = store.BulkInsert(token: ct))
        {
            foreach (var doc in plan.Documents)
                await bulk.StoreAsync(doc.Entity, doc.Id);
        }
        log.WriteLine($"  bulk-inserted {plan.Documents.Count} documents");

        // Same raw string values UserStore.CreateEmailReservationAsync writes (PutCompareExchangeValueOperation<string>),
        // batched into cluster-wide transactions instead of one Raft round-trip per user. Creating (not
        // updating) means an already-taken key fails the batch.
        foreach (var chunk in plan.CompareExchange.Chunk(500))
        {
            using var session = store.OpenAsyncSession(new SessionOptions { TransactionMode = TransactionMode.ClusterWide });
            foreach (var (key, value) in chunk)
                session.Advanced.ClusterTransaction.CreateCompareExchangeValue(key, value);
            await session.SaveChangesAsync(ct);
        }
        log.WriteLine($"  wrote {plan.CompareExchange.Count} compare-exchange email reservations");
    }

    public async Task DeployIndexesAsync(TimeSpan timeout, CancellationToken ct = default)
    {
        var assembly = typeof(MintPlayer.Web.MintPlayerUser).Assembly;
        await IndexCreation.CreateIndexesAsync(assembly, store, token: ct);
        var sw = Stopwatch.StartNew();
        while (true)
        {
            var stats = await store.Maintenance.SendAsync(new GetStatisticsOperation(), ct);
            if (stats.StaleIndexes.Length == 0)
            {
                log.WriteLine($"  {stats.CountOfIndexes} indexes deployed from {assembly.GetName().Name}, non-stale after {sw.Elapsed.TotalSeconds:0.0}s");
                return;
            }
            if (sw.Elapsed > timeout)
                throw new TimeoutException($"indexes still stale after {timeout}: {string.Join(", ", stats.StaleIndexes)}");
            await Task.Delay(200, ct);
        }
    }
}
