using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public partial class LevelMap
{
    [Key(0)]
    public byte[] Terrain { get; set; } = [];

    [Key(1)]
    public byte[] WallNeighbors { get; set; } = [];

    [Key(2)]
    public byte[] VisibleTerrain { get; set; } = [];
}
