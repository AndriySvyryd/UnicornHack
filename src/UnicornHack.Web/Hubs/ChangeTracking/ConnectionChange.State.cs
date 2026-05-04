using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

public partial class ConnectionChange : IChangeWithState
{
    [IgnoreMember]
    public GameEntity Entity
    {
        get; set;
    } = null!;

    [IgnoreMember]
    public EntityState? State { get; set; }
}
