using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public class RoomMap
{
    [Key(0)]
    public MapFeature[] Terrain { get; set; } = [];

    [Key(1)]
    public DirectionFlags[] WallNeighbors { get; set; } = [];

    [Key(2)]
    public byte[] VisibleTerrain { get; set; } = [];
}
