using System.Collections.Generic;
using UnicornHack.Services;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack
{
    public class TestRepository : IRepository
    {
        public T Find<T>(params object[] keyValues) where T : class
            => default;

        public void Add<T>(T entity) where T : class
        {
            if (entity is ITrackable trackable)
            {
                trackable.StartTracking(this);
            }
        }

        public void Remove(object entity)
        {
        }

        public void RemoveTracked(object entity)
        {
            if (entity is ITrackable trackable)
            {
                trackable.StopTracking(this);
            }
        }

        public void LoadLevels(IReadOnlyList<int> levelIds)
        {
        }
    }
}
