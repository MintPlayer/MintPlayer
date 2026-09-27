using MintPlayer.Domain.Entities;
using MintPlayer.Spark.Abstractions;
using MintPlayer.Spark.Actions;
using MintPlayer.Spark.Services;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace MintPlayer.Web.Actions;

/// <summary>
/// Shared Actions base for every <see cref="Entity"/>-derived type. Implements the step-1.4
/// conventions once so per-entity Actions classes only add genuine business logic:
/// <list type="bullet">
///   <item><see cref="OnBeforeSaveAsync"/> stamps <see cref="Entity.CreatedAt"/> on create and
///   <see cref="Entity.ModifiedAt"/> on every subsequent edit.</item>
///   <item><see cref="OnDeleteAsync"/> soft-deletes (flag + timestamp) instead of physically
///   removing the document.</item>
///   <item><see cref="OnLoadAsync"/> hides soft-deleted rows (a delete therefore reads back as 404).</item>
///   <item><see cref="OnQueryAsync"/> filters soft-deleted rows out of Actions-driven list results.</item>
/// </list>
/// The named-query list path (the datatable's <c>Database.*</c> source) does not flow through
/// this class; it is filtered separately at the <c>SparkContext</c> queryable
/// (see <c>MintPlayerSparkContext</c>) so deleted rows never surface there either.
///
/// The class is <c>abstract</c>, so the framework's actions-registration generator skips it
/// (it walks the inheritance chain to find <c>DefaultPersistentObjectActions&lt;T&gt;</c> but
/// excludes abstract types); only the concrete <c>{Entity}Actions</c> subclasses get registered.
/// </summary>
public abstract class EntityActions<T> : DefaultPersistentObjectActions<T> where T : Entity
{
    protected EntityActions(IEntityMapper entityMapper) : base(entityMapper) { }

    /// <summary>Clock seam — override in tests to make audit timestamps deterministic.</summary>
    protected virtual DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    public override Task OnBeforeSaveAsync(PersistentObject obj, T entity)
    {
        var now = UtcNow;
        if (string.IsNullOrEmpty(obj.Id))
            entity.CreatedAt = now;
        else
            entity.ModifiedAt = now;

        return Task.CompletedTask;
    }

    public override async Task<T?> OnLoadAsync(IAsyncDocumentSession session, string id)
    {
        var entity = await base.OnLoadAsync(session, id);
        return entity is { IsDeleted: false } ? entity : null;
    }

    public override async Task<IEnumerable<T>> OnQueryAsync(IAsyncDocumentSession session)
        => await session.Query<T>().Where(x => !x.IsDeleted).ToListAsync();

    public override async Task OnDeleteAsync(IAsyncDocumentSession session, string id)
    {
        var entity = await session.LoadAsync<T>(id);
        if (entity is null || entity.IsDeleted)
            return;

        await OnBeforeDeleteAsync(entity);
        entity.IsDeleted = true;
        entity.DeletedAt = UtcNow;
        await session.SaveChangesAsync();
    }
}
