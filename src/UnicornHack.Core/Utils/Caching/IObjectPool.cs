namespace UnicornHack.Utils.Caching
{
    public interface IObjectPool
    {
        object Get();
        void Return(object obj);
    }
}
