using UnicornHack.Data;
using UnicornHack.Services;

namespace UnicornHack.Hubs
{
    public class SerializationContext
    {
        public SerializationContext(
            GameDbContext dbContext, GameEntity observer, GameServices services)
        {
            DbContext = dbContext;
            Observer = observer;
            Manager = observer.Manager;
            Services = services;
        }

        public GameDbContext DbContext { get; }
        public GameEntity Observer { get; }
        public GameManager Manager { get; }
        public GameServices Services { get; }
    }
}
