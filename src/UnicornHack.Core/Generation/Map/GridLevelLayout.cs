using UnicornHack.Utils;

namespace UnicornHack.Generation.Map;

/// <summary>
///     4-neighbour grid layout. Rooms occupy cells of a <see cref="LayoutParameters.GridWidth" />
///     × <see cref="LayoutParameters.GridHeight" /> rectangle and are connected only to their
///     N/E/S/W neighbours. Most common topology — cardinal corridors, no diagonal traversal.
/// </summary>
public class GridLevelLayout : LevelLayout
{
    public GridLevelLayout()
    {
        GridHeight = 3;
        GridWidth = 5;
        Interconnectedness = 0.75f;
        MinRooms = 15;
        MaxRooms = 20;
    }

    public override IEnumerable<(int dx, int dy)> GetNeighbourOffsets(byte cellX, byte cellY)
    {
        yield return (1, 0);
        yield return (-1, 0);
        yield return (0, 1);
        yield return (0, -1);
    }

    /// <summary>
    ///     Entrance at column 0 and exit at the last column, both at independently chosen
    ///     random rows. Every other cell is sampled with <see cref="Density" /> in
    ///     shuffled order, then the resulting set is capped at
    ///     <see cref="LayoutParameters.MaxRooms" /> via
    ///     <see cref="LevelLayout.TrimRooms" />. The cap honours the neighbour-degree
    ///     rule (a node is only dropped if every surviving neighbour keeps at least two
    ///     neighbours), so connectivity between entrance and exit is preserved without an
    ///     explicit backbone reservation.
    /// </summary>
    protected override List<RoomGraphNode> PlaceRooms(LayoutParameters parameters, SimpleRandom random)
    {
        var nodes = new List<RoomGraphNode>(parameters.MaxRooms);
        var byCell = new RoomGraphNode?[parameters.GridWidth, parameters.GridHeight];

        var entranceY = (byte)random.Next(parameters.GridHeight);
        var entrance = new RoomGraphNode { GridX = 0, GridY = entranceY, Features = RoomFeatures.Entrance };
        byCell[0, entranceY] = entrance;
        nodes.Add(entrance);

        var exitX = (byte)(parameters.GridWidth - 1);
        var exitY = (byte)random.Next(parameters.GridHeight);
        var exit = new RoomGraphNode { GridX = exitX, GridY = exitY, Features = RoomFeatures.Exit };
        byCell[exitX, exitY] = exit;
        nodes.Add(exit);

        // Optional cells, shuffled so the MaxRooms cap doesn't bias retained rooms to one
        // corner of the grid. Cells with fewer than two in-grid topology neighbours are
        // skipped — placing them would just create dead-ends that BuildGraph rejects.
        var optional = new List<(byte x, byte y)>();
        for (byte x = 0; x < parameters.GridWidth; x++)
        {
            for (byte y = 0; y < parameters.GridHeight; y++)
            {
                if (byCell[x, y] is null
                    && CountInGridNeighbours(x, y, parameters.GridWidth, parameters.GridHeight) >= 2)
                {
                    optional.Add((x, y));
                }
            }
        }
        Shuffle(optional, random);

        foreach (var (x, y) in optional)
        {
            if (random.NextSingle(0f, 1f) < parameters.Density)
            {
                var node = new RoomGraphNode { GridX = x, GridY = y };
                byCell[x, y] = node;
                nodes.Add(node);
            }
        }

        return nodes;
    }
}
