using Microsoft.Extensions.Caching.Memory;

namespace UnicornHack.Services
{
    public class GameServices
    {
        public GameServices(ILanguageService language, IMemoryCache sharedCache)
        {
            Language = language;
            SharedCache = sharedCache;
        }

        public virtual ILanguageService Language { get; }
        public virtual IMemoryCache SharedCache { get; }
    }
}