using UnicornHack.Utils;

namespace UnicornHack.Generation.Map;

/// <summary>
///     6-neighbour offset-hex layout. Rows are horizontally staggered by half a column so each
///     cell has six neighbours: 3 on the left and 3 on the right.
///     There are no vertical-only neighbours;.
/// </summary>
/// <remarks>
///     Uses "odd-r horizontal layout" offset coordinates. For an odd row the diagonal
///     neighbours are <c>(0, ±1)</c> and <c>(1, ±1)</c>; for an even row they are
///     <c>(0, ±1)</c> and <c>(-1, ±1)</c>. Row parity is decided by <c>cellY &amp; 1</c>.
/// </remarks>
public class HexLevelLayout : LevelLayout
{
    public HexLevelLayout()
    {
        GridHeight = 5;
        GridWidth = 7;
        Interconnectedness = 0.75f;
        Sinuosity = 0.2f;
        MinRooms = 28;
        MaxRooms = 32;
    }

    public override IEnumerable<(int dx, int dy)> GetNeighbourOffsets(byte cellX, byte cellY)
    {
        // Horizontal neighbours
        yield return (-1, 0);
        yield return (1, 0);

        // Same column neighbours
        yield return (0, 1);
        yield return (0, -1);

        if ((cellY & 1) == 0)
        {
            // Even row: diagonal neighbours are in the previous column
            yield return (-1, 1);
            yield return (-1, -1);
        }
        else
        {
            // Odd row: diagonal neighbours are in the next column
            yield return (1, 1);
            yield return (1, -1);
        }
    }

    /// <summary>
    ///     Entrance at column 0 and exit at the last column, both at independently chosen
    ///     random rows. The remaining cells are sampled in row-major order with
    ///     <see cref="Density" /> probability and placement stops when
    ///     <see cref="LayoutParameters.MaxRooms" /> is reached. Hex layouts deliberately keep
    ///     the row-major bias for optional cells because the 6-neighbour topology is sparse:
    ///     scattering cells uniformly across the grid leaves too few edge candidates for
    ///     <see cref="LevelLayout.BuildGraph" /> to find a strongly-connected solution.
    /// </summary>
    protected override List<RoomGraphNode> PlaceRooms(LayoutParameters parameters, SimpleRandom random)
    {
        var nodes = new List<RoomGraphNode>();
        var occupied = new HashSet<(byte X, byte Y)>();

        var entranceY = (byte)random.Next(0, parameters.GridHeight);
        nodes.Add(new RoomGraphNode { GridX = 0, GridY = entranceY, Features = RoomFeatures.Entrance });
        occupied.Add((0, entranceY));

        var exitX = (byte)(parameters.GridWidth - 1);
        var exitY = (byte)random.Next(0, parameters.GridHeight);
        nodes.Add(new RoomGraphNode { GridX = exitX, GridY = exitY, Features = RoomFeatures.Exit });
        occupied.Add((exitX, exitY));

        for (byte x = 0; x < parameters.GridWidth; x++)
        {
            for (byte y = 0; y < parameters.GridHeight; y++)
            {
                if (occupied.Contains((x, y)))
                {
                    continue;
                }

                // Skip cells with fewer than two in-grid topology neighbours — they can't
                // satisfy the "≥ 2 neighbours" invariant BuildGraph enforces.
                if (CountInGridNeighbours(x, y, parameters.GridWidth, parameters.GridHeight) < 2)
                {
                    continue;
                }

                if (random.NextSingle(0f, 1f) < parameters.Density)
                {
                    nodes.Add(new RoomGraphNode { GridX = x, GridY = y });
                    occupied.Add((x, y));
                }
            }
        }

        return nodes;
    }
}
