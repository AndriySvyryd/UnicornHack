using UnicornHack.Services;

namespace UnicornHack
{
    public class TestRepository : IRepository
    {
        public T Find<T>(params object[] keyValues) where T : class
            => default;

        public void Add<T>(T entity) where T : class
        {
        }

        public void Delete<T>(T entity) where T : class
        {
        }
    }
}