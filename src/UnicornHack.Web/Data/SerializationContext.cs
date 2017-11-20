using UnicornHack.Services;

namespace UnicornHack.Data
{
    public class SerializationContext
    {
        public SerializationContext(GameDbContext context, Player observer, GameServices services)
        {
            Context = context;
            Observer = observer;
            Services = services;
        }

        public GameDbContext Context { get; }
        public Player Observer { get; }
        public GameServices Services { get; }
    }
}