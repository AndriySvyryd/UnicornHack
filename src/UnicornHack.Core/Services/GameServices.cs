namespace UnicornHack.Services
{
    public class GameServices
    {
        public GameServices(LanguageService language)
        {
            Language = language;
        }

        public virtual LanguageService Language { get; }
    }
}