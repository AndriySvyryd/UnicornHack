namespace UnicornHack.Utils.DataLoading;

/// <summary>
///     Read abstraction over a name-keyed table of authored game data
///     (<see cref="Loader{T}" /> is the production implementation). Exposed via
///     <see cref="Services.IDataProvider" /> so tests can swap in providers that
///     hold a different mix of data — typically the production set plus extra synthetic
///     entries — without touching the static singletons. Production loaders are immutable
///     after their initial load; test variants compose synthetic data with production data
///     in their own implementations.
/// </summary>
public interface ILoader<T> where T : class, ILoadable
{
    T Get(string name);
    T? Find(string name);
    IEnumerable<T> GetAll();
    IReadOnlyList<T> GetAsList();
}

/// <summary><see cref="ILoader{T}" /> with a unique secondary index.</summary>
public interface IIndexedLoader<TIndexKey, T> : ILoader<T>
    where T : class, ILoadable
    where TIndexKey : notnull
{
    T? FindByIndex(TIndexKey key);
}

/// <summary>
///     <see cref="ILoader{T}" /> with a secondary index keyed by <typeparamref name="TKey" />.
/// </summary>
public interface IGroupedLoader<TKey, T> : ILoader<T>
    where T : class, ILoadable
    where TKey : notnull
{
    IReadOnlyList<T> GetAllValues(TKey key);
    IEnumerable<TKey> GetAllKeys();
}

/// <summary><see cref="IGroupedLoader{TKey,T}" /> with an additional unique secondary index.</summary>
public interface IIndexedGroupedLoader<TKey, TIndexKey, T> :
    IGroupedLoader<TKey, T>, IIndexedLoader<TIndexKey, T>
    where T : class, ILoadable
    where TKey : notnull
    where TIndexKey : notnull
{
}

/// <summary>
///     <see cref="ILoader{T}" /> for sources known to hold exactly one canonical value
///     (e.g. <see cref="ItemGroup" />).
/// </summary>
public interface ISingleLoader<T> : ILoader<T> where T : class, ILoadable, new()
{
    T Object { get; }
}
