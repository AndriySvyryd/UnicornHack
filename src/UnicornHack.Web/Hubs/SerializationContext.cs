using System.Collections;
using UnicornHack.Data;
using UnicornHack.Services;

namespace UnicornHack.Hubs;

public class SerializationContext
{
    public static readonly BitArray DeletedBitArray = new(1);

    public SerializationContext(GameDbContext dbContext, GameEntity observer, GameServices services)
    {
        DbContext = dbContext;
        Observer = observer;
        Manager = observer.Manager;
        Services = services;
    }

    public GameDbContext DbContext
    {
        get;
    }

    public GameEntity Observer
    {
        get;
    }

    public GameManager Manager
    {
        get;
    }

    public GameServices Services
    {
        get;
    }
}
