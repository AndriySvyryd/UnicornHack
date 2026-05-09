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

/// <summary>
///     <see cref="ILoader{T}" /> for sources known to hold exactly one canonical value
///     (e.g. <see cref="UnicornHack.Generation.ItemGroup" />).
/// </summary>
public interface ISingleLoader<T> : ILoader<T> where T : class, ILoadable, new()
{
    T Object { get; }
}
