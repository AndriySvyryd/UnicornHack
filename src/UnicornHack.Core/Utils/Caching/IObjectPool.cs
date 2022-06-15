namespace UnicornHack.Utils.Caching;

public interface IObjectPool
{
    object Rent();
    void Return(object obj);
}
