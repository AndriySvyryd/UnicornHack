using Microsoft.Extensions.Caching.Memory;

namespace UnicornHack.Services;

public class GameServices
{
    public GameServices(ILanguageService language, IMemoryCache sharedCache, IDataProvider? data = null)
    {
        Language = language;
        SharedCache = sharedCache;
        Data = data ?? DataProvider.Default;
    }

    public ILanguageService Language
    {
        get;
    }

    public IMemoryCache SharedCache
    {
        get;
    }

    /// <summary>
    ///     Game-scoped accessor for the parsed CS-script data (creatures, items, abilities,
    ///     branches, fragments, …). Defaults to <see cref="DataProvider.Default" /> which
    ///     forwards to the static <c>*.Loader</c> singletons; tests substitute custom
    ///     providers to isolate per-test mutations of <c>DefiningMapFragments</c>.
    /// </summary>
    public IDataProvider Data
    {
        get;
    }
}
