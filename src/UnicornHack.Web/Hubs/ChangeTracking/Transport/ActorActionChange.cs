using MessagePack;
using UnicornHack.Systems.Actors;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public class ActorActionChange
{
    [Key(0)]
    public ActorActionType Type { get; set; }

    [Key(1)]
    public string? Name { get; set; }

    [Key(2)]
    public int Target { get; set; }

    [Key(3)]
    public TargetingShape TargetingShape { get; set; }

    [Key(4)]
    public int TargetingShapeSize { get; set; }
}
