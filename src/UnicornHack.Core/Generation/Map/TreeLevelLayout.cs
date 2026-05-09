using CSharpScriptSerialization;
using UnicornHack.Utils;

namespace UnicornHack.Generation.Map;

/// <summary>
///     A <see cref="GridLevelLayout" /> tuned for the classic dungeon backbone: rooms placed
///     column-by-column from a single entrance at column 0 to a single exit at column
///     <see cref="GridLevelLayout.GridWidth" /> - 1, with optional fork branches into adjacent
///     rows. <see cref="GridLevelLayout.Interconnectedness" /> adds extra cross-connections beyond
///     the spanning tree (0.0 = nearly linear, 1.0 = grid-like).
/// </summary>
/// <remarks>
///     Inherits the topology-agnostic algorithm from <see cref="LevelLayout.BuildGraph" /> via
///     <see cref="GridLevelLayout" /> but overrides <see cref="GetNeighbourOffsets" /> to use
///     all 8 neighbours, so the prune-from-full algorithm can keep diagonal cross-shortcuts
///     between rooms in addition to the cardinal backbone. <see cref="PlaceRooms" /> is also
///     overridden to bias the spanning tree left-to-right via a column frontier.
/// </remarks>
public class TreeLevelLayout : GridLevelLayout, ICSScriptSerializable
{
    /// <summary>
    ///     8-neighbour topology (cardinals + diagonals).
    /// </summary>
    public override IEnumerable<(int dx, int dy)> GetNeighbourOffsets(byte cellX, byte cellY)
    {
        for (var dy = -1; dy <= 1; dy++)
        {
            for (var dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0)
                {
                    continue;
                }

                yield return (dx, dy);
            }
        }
    }

    protected const byte DefaultGridWidth = 7;
    protected const byte DefaultGridHeight = 5;
    protected const float DefaultInterconnectedness = 0.75f;
    protected const float DefaultRewardRoomProbability = 0.4f;
    protected const float DefaultCurseRoomProbability = 0.15f;
    protected const float DefaultHiddenConnectionProbability = 0.1f;
    protected const float DefaultSinuosity = 0f;
    protected const byte DefaultMinRooms = 28;
    protected const byte DefaultMaxRooms = 32;

    public TreeLevelLayout()
    {
        GridWidth = DefaultGridWidth;
        GridHeight = DefaultGridHeight;
        Interconnectedness = DefaultInterconnectedness;
        RewardRoomProbability = DefaultRewardRoomProbability;
        CurseRoomProbability = DefaultCurseRoomProbability;
        HiddenConnectionProbability = DefaultHiddenConnectionProbability;
        Sinuosity = DefaultSinuosity;
        MinRooms = DefaultMinRooms;
        MaxRooms = DefaultMaxRooms;
    }

    /// <summary>
    ///     Fills every cell of the <see cref="GridLevelLayout.GridWidth" /> ×
    ///     <see cref="GridLevelLayout.GridHeight" /> grid, then anchors the entrance to a
    ///     random row of column 0 and the exit to a random row of the last column.
    ///     <see cref="LevelLayout.TrimRooms" /> downstream decides which rooms to drop and
    ///     how many to keep, biasing toward retaining edge-disjoint entrance→exit paths.
    /// </summary>
    protected override List<RoomGraphNode> PlaceRooms(LayoutParameters parameters, SimpleRandom random)
    {
        var nodes = new List<RoomGraphNode>(parameters.GridWidth * parameters.GridHeight);
        for (byte x = 0; x < parameters.GridWidth; x++)
        {
            for (byte y = 0; y < parameters.GridHeight; y++)
            {
                nodes.Add(new RoomGraphNode { GridX = x, GridY = y });
            }
        }

        var entranceY = (byte)random.Next(parameters.GridHeight);
        var entrance = nodes.Find(n => n.GridX == 0 && n.GridY == entranceY)!;
        entrance.Features |= RoomFeatures.Entrance;

        var exitY = (byte)random.Next(parameters.GridHeight);
        var exitX = (byte)(parameters.GridWidth - 1);
        var exit = nodes.Find(n => n.GridX == exitX && n.GridY == exitY)!;
        exit.Features |= RoomFeatures.Exit;

        return nodes;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<TreeLevelLayout>(GetPropertyConditions<TreeLevelLayout>());

    public ICSScriptSerializer GetSerializer() => Serializer;

    protected static Dictionary<string, Func<TTreeLayout, object?, bool>> GetPropertyConditions<TTreeLayout>()
        where TTreeLayout : TreeLevelLayout => new()
    {
        // ReSharper disable CompareOfFloatsByEqualityOperator
        { nameof(MaxRooms), (_, v) => (byte)v! != DefaultMaxRooms },
        { nameof(GridWidth), (_, v) => (byte)v! != DefaultGridWidth },
        { nameof(GridHeight), (_, v) => (byte)v! != DefaultGridHeight },
        { nameof(Interconnectedness), (_, v) => (float)v! != DefaultInterconnectedness },
        { nameof(RewardRoomProbability), (_, v) => (float)v! != DefaultRewardRoomProbability },
        { nameof(CurseRoomProbability), (_, v) => (float)v! != DefaultCurseRoomProbability },
        { nameof(HiddenConnectionProbability), (_, v) => (float)v! != DefaultHiddenConnectionProbability },
        { nameof(MinRooms), (_, v) => (byte)v! != DefaultMinRooms }
        // ReSharper restore CompareOfFloatsByEqualityOperator
    };
}

// TODO: HubAndSpokeLayout
// Central hub room connected to all other rooms via radiating spokes.
// Each spoke ends in a specialized room (shrines, pedestals, challenge).
// Additional parameters: SpokeCount (number of radiating paths), SpokeLength,
// HubSize (cells reserved for the hub). The hub doubles as a safe zone where
// the player can return between spoke runs.
// Good for: Norse (Hel's domain), Hindu temple.

// TODO: CascadeLayout
// Wide entrance (multiple parallel rooms in the early columns) funneling to a
// single boss room in the final column. Each column has fewer rooms than the
// previous. All connections flow strictly left-to-right; no vertical connections.
// Additional parameters: EntranceWidth (rooms in column 0), NarrowingRate (how
// fast the column count shrinks). Encourages decisive route choices early.
// Good for: Japanese, Slavic (narrowing toward Koschei).
