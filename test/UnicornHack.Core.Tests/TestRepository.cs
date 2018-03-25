using System.Collections.Generic;
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

        public void DeleteTracked<T>(T entity) where T : class
        {
        }

        public void Remove(object entity)
        {
        }

        public void RemoveTracked(object entity)
        {
        }

        public void LoadLevels(IReadOnlyList<int> levelIds)
        {
        }
    }
}
