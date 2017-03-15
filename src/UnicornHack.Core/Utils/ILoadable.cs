namespace UnicornHack.Utils
{
    public interface ILoadable
    {
        string Name { get; }
        void OnLoad();
    }
}