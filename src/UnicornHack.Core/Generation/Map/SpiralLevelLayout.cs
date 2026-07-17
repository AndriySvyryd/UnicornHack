using UnicornHack.Utils;

namespace UnicornHack.Generation.Map;

/// <summary>
///     8-neighbor spiral layout. Rooms are placed along an outward Archimedean spiral starting
///     near the centre of the grid; the entrance is the first room emitted and the exit is the
///     last. Diagonal connections are allowed, producing dense graphs with cross-shortcut
///     potential.
/// </summary>
public class SpiralLevelLayout : LevelLayout
{
    public SpiralLevelLayout()
    {
        GridHeight = 6;
        Interconnectedness = 0.7f;
        CurseRoomProbability = 0.15f;
        Sinuosity = 0.5f;
        MaxRooms = 30;
    }

    /// <summary>
    ///     When <c>false</c> (default) the entrance is at the centre of the spiral and the
    ///     exit at the outermost cell. When <c>true</c> the two are swapped — entrance on the
    ///     outer rim, exit at the spiral's centre. Useful for branches that want the player to
    ///     descend toward a central destination rather than spiral outward.
    /// </summary>
    public bool SwapEntranceAndExit { get; init; }

    /// <summary>Spiral placement doesn't use density — all cells along the spiral are placed.</summary>
    public override float ComputeDensity() => 1f;

    public override IEnumerable<(int dx, int dy)> GetNeighborOffsets(byte cellX, byte cellY)
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

    /// <summary>
    ///     Walks an outward four-direction spiral starting from the centre cell.
    ///     Entrance/exit roles can be swapped via <see cref="SwapEntranceAndExit" />.
    /// </summary>
    protected override List<RoomGraphNode> PlaceRooms(LayoutParameters parameters, SimpleRandom random)
    {
        var nodes = new List<RoomGraphNode>(parameters.MaxRooms);
        var cx = (byte)(parameters.GridWidth / 2);
        var cy = (byte)(parameters.GridHeight / 2);

        var x = (int)cx;
        var y = (int)cy;
        // Spiral state: heading and segment length grow every two turns.
        // 0=East, 1=South, 2=West, 3=North.
        var heading = 0;
        var segmentLength = 1;
        var segmentSteps = 0;
        var turnsSinceGrow = 0;

        nodes.Add(new RoomGraphNode { GridX = (byte)x, GridY = (byte)y });
        while (nodes.Count < parameters.MaxRooms)
        {
            var (dx, dy) = heading switch
            {
                0 => (1, 0),
                1 => (0, 1),
                2 => (-1, 0),
                _ => (0, -1)
            };

            x += dx;
            y += dy;
            segmentSteps++;

            if (x < 0 || y < 0 || x >= parameters.GridWidth || y >= parameters.GridHeight)
            {
                // Spiral has left the grid — nothing more to place.
                break;
            }

            nodes.Add(new RoomGraphNode { GridX = (byte)x, GridY = (byte)y });

            if (segmentSteps == segmentLength)
            {
                heading = (heading + 1) & 3;
                segmentSteps = 0;
                turnsSinceGrow++;
                if (turnsSinceGrow == 2)
                {
                    segmentLength++;
                    turnsSinceGrow = 0;
                }
            }
        }

        // Tag the spiral endpoints. By default the centre (first node) is the entrance and
        // the outermost reached cell (last node) is the exit; SwapEntranceAndExit flips them.
        var (entranceIndex, exitIndex) = SwapEntranceAndExit ? (nodes.Count - 1, 0) : (0, nodes.Count - 1);
        nodes[entranceIndex].Features |= RoomFeatures.Entrance;
        nodes[exitIndex].Features |= RoomFeatures.Exit;

        return nodes;
    }
}
