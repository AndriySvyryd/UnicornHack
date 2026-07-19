using UnicornHack.Utils;
using UnicornHack.Utils.DataLoading;

namespace UnicornHack;

/// <summary>
///     <see cref="IGroupedLoader{TKey, T}" /> variant of <see cref="TestLoader{T}" />. The
///     key selector is supplied at construction so test items added via <see cref="TestLoader{T}.Add" />
///     show up under the right key in <see cref="GetAllValues" /> and <see cref="GetAllKeys" />.
/// </summary>
public class TestGroupedLoader<TKey, T> : TestLoader<T>, IGroupedLoader<TKey, T>
    where T : class, ILoadable
    where TKey : notnull
{
    private readonly Func<T, IEnumerable<TKey>> _keySelector;
    private readonly IGroupedLoader<TKey, T>? _production;

    public TestGroupedLoader(Func<T, TKey> keySelector, IGroupedLoader<TKey, T>? production = null)
        : this(i => Sequence.Single(keySelector(i)), production)
    {
    }

    public TestGroupedLoader(Func<T, IEnumerable<TKey>> keySelector, IGroupedLoader<TKey, T>? production = null)
        : base(production)
    {
        _keySelector = keySelector;
        _production = production;
    }

    public IReadOnlyList<T> GetAllValues(TKey key)
    {
        var result = new List<T>();
        foreach (var item in TestItems)
        {
            foreach (var itemKey in _keySelector(item))
            {
                if (EqualityComparer<TKey>.Default.Equals(itemKey, key))
                {
                    result.Add(item);
                    break;
                }
            }
        }

        if (IncludeProduction && _production != null)
        {
            foreach (var item in _production.GetAllValues(key))
            {
                if (!IsShadowed(item))
                {
                    result.Add(item);
                }
            }
        }

        return result;
    }

    public IEnumerable<TKey> GetAllKeys()
    {
        var seen = new HashSet<TKey>();
        foreach (var item in TestItems)
        {
            foreach (var key in _keySelector(item))
            {
                if (seen.Add(key))
                {
                    yield return key;
                }
            }
        }

        if (!IncludeProduction || _production == null)
        {
            yield break;
        }

        foreach (var key in _production.GetAllKeys())
        {
            if (seen.Add(key))
            {
                yield return key;
            }
        }
    }
}
