using UnicornHack.Utils.DataLoading;

namespace UnicornHack;

/// <summary>
///     <see cref="ILoader{T}" /> implementation for tests. Wraps an optional <em>production</em>
///     loader and adds an in-memory bag of synthetic test items.
///     <list type="bullet">
///         <item>
///             <description>
///                 <see cref="IncludeProduction" /> is <c>true</c> (the default) — test items
///                 are layered on top of production data. <see cref="Find" /> prefers test
///                 items by name; production data is visible for names not in the test set.
///             </description>
///         </item>
///         <item>
///             <description>
///                 <see cref="IncludeProduction" /> is <c>false</c> — only the test items are
///                 visible. Switch the flag at any time; queries see the new mode on the next
///                 call.
///             </description>
///         </item>
///     </list>
///     Tests add data via <see cref="Add" /> / <see cref="AddRange" /> after construction.
/// </summary>
public class TestLoader<T> : ILoader<T> where T : class, ILoadable
{
    private readonly List<T> _testItems = [];

    public TestLoader(ILoader<T>? production = null) => Production = production;

    /// <summary>The wrapped production loader, if any.</summary>
    public ILoader<T>? Production { get; }

    /// <summary>
    ///     When <c>true</c> (default), queries fall through to <see cref="Production" /> after
    ///     consulting the test items. Set to <c>false</c> to hide production data entirely.
    /// </summary>
    public bool IncludeProduction { get; set; } = true;

    /// <summary>Synthetic items added by the test, in insertion order.</summary>
    public IReadOnlyList<T> TestItems => _testItems;

    public void Add(T item) => _testItems.Add(item);

    public void AddRange(IEnumerable<T> items) => _testItems.AddRange(items);

    public T Get(string name)
        => Find(name) ?? throw new InvalidOperationException($"{typeof(T).Name} '{name}' not found");

    public T? Find(string name)
    {
        foreach (var item in _testItems)
        {
            if (string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }
        }

        return IncludeProduction ? Production?.Find(name) : null;
    }

    public IEnumerable<T> GetAll()
    {
        foreach (var item in _testItems)
        {
            yield return item;
        }

        if (!IncludeProduction || Production == null)
        {
            yield break;
        }

        foreach (var item in Production.GetAll())
        {
            // Names in the test set shadow production entries with the same name.
            var shadowed = false;
            foreach (var test in _testItems)
            {
                if (string.Equals(test.Name, item.Name, StringComparison.OrdinalIgnoreCase))
                {
                    shadowed = true;
                    break;
                }
            }

            if (!shadowed)
            {
                yield return item;
            }
        }
    }

    public IReadOnlyList<T> GetAsList() => GetAll().ToList();
}
