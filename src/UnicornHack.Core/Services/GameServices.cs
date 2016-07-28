namespace UnicornHack.Services
{
    public class GameServices
    {
        public GameServices(ILanguageService language)
        {
            Language = language;
        }

        public virtual ILanguageService Language { get; }
    }
}