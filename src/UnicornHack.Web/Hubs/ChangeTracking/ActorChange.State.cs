using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

public partial class ActorChange : IChangeWithState
{
    [IgnoreMember]
    public GameEntity Entity
    {
        get; set;
    } = null!;

    [IgnoreMember]
    public EntityState? State { get; set; }
}
