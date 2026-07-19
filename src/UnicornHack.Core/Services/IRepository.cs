using UnicornHack.Systems.Levels;

namespace UnicornHack.Services;

public interface IRepository
{
    T? Find<T>(params object[] keyValues) where T : class;
    void Add<T>(T entity) where T : class;
    void Remove(object entity);
    void RemoveTracked(object entity);
    void LoadRooms(IReadOnlyList<int> roomIds);
    void DeleteLevel(GameLevel level);
}
