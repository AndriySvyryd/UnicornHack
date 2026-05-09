using UnicornHack.Utils.DataLoading;

namespace UnicornHack;

/// <summary>
///     <see cref="ISingleLoader{T}" /> variant of <see cref="TestLoader{T}" />. <see cref="Object" />
///     prefers the first test item if any have been added; otherwise falls through to the
///     production loader (when <see cref="TestLoader{T}.IncludeProduction" /> is true).
/// </summary>
public class TestSingleLoader<T> : TestLoader<T>, ISingleLoader<T>
    where T : class, ILoadable, new()
{
    private readonly ISingleLoader<T>? _production;

    public TestSingleLoader(ISingleLoader<T>? production = null) : base(production)
        => _production = production;

    public T Object
    {
        get
        {
            if (TestItems.Count > 0)
            {
                return TestItems[0];
            }

            if (IncludeProduction && _production != null)
            {
                return _production.Object;
            }

            throw new InvalidOperationException(
                $"{typeof(T).Name} test loader has no test items and no production fallback");
        }
    }
}
