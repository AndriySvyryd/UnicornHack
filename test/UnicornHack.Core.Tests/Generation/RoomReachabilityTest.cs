using UnicornHack.Data.Branches;
using UnicornHack.Primitives;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation;

/// <summary>
///     Verifies that every walkable tile in a generated room is reachable from the
///     room's entrance/connection points via 4-directional movement through walkable
///     terrain. Catches corridor-carving regressions that leave chunks disconnected.
/// </summary>
public class RoomReachabilityTest
{
    public static IEnumerable<object[]> Layouts()
    {
        yield return [new GridLevelLayout()];
        yield return [new HexLevelLayout()];
        yield return [new SpiralLevelLayout()];
    }

    [Theory]
    [MemberData(nameof(Layouts))]
    public void Every_walkable_tile_is_reachable_from_an_entrance(LevelLayout layout)
    {
        var success = false;
        for (uint seed = 1; seed <= 8 && !success; seed++)
        {
            var game = TestHelper.CreateGame(seed);
            var manager = game.Manager;
            var branch = new Branch
            {
                Name = BranchData.Dungeon.Name,
                Length = BranchData.Dungeon.Length,
                Layout = layout
            };
            ((TestDataProvider)game.Services.Data).Branches.Add(branch);
            var dungeon = branch.Instantiate(game);
            var level = LevelGenerator.CreateEmpty(dungeon, depth: 2, seed, manager);

            try
            {
                if (!LevelGenerator.EnsureGenerated(level))
                {
                    continue;
                }
            }
            catch (InvalidOperationException)
            {
                continue;
            }

            foreach (var room in level.Rooms)
            {
                AssertEveryWalkableTileReachable(room, layout, seed);
            }

            success = true;
        }

        Assert.True(success, $"{layout.GetType().Name} failed to produce a valid level in 8 seeds");
    }

    /// <summary>
    ///     BFS from the room's connection tiles across walkable terrain and verify it
    ///     reaches every other walkable tile in the room.
    /// </summary>
    private static void AssertEveryWalkableTileReachable(RoomComponent room, LevelLayout layout, uint seed)
    {
        var starts = new List<Point>(room.Connections.Keys);
        if (starts.Count == 0)
        {
            // Room with no inter-room connections — start the BFS from any walkable tile.
            for (byte y = 0; y < room.Height && starts.Count == 0; y++)
            {
                for (byte x = 0; x < room.Width && starts.Count == 0; x++)
                {
                    var feature = room.Terrain[room.PointToIndex![x, y]];
                    if (feature.CanMoveTo())
                    {
                        starts.Add(new Point(x, y));
                    }
                }
            }
        }

        if (starts.Count == 0)
        {
            return;
        }

        var visited = new bool[room.TileCount];
        var queue = new Queue<Point>();
        foreach (var start in starts)
        {
            var startIdx = room.PointToIndex![start.X, start.Y];
            if (!visited[startIdx])
            {
                visited[startIdx] = true;
                queue.Enqueue(start);
            }
        }

        while (queue.Count > 0)
        {
            var p = queue.Dequeue();
            for (var d = 0; d < 4; d++)
            {
                var dx = d == 0 ? 1 : d == 1 ? -1 : 0;
                var dy = d == 2 ? 1 : d == 3 ? -1 : 0;
                var nx = p.X + dx;
                var ny = p.Y + dy;
                if (nx < 0 || ny < 0 || nx >= room.Width || ny >= room.Height)
                {
                    continue;
                }

                var nIdx = room.PointToIndex![(byte)nx, (byte)ny];
                if (visited[nIdx])
                {
                    continue;
                }

                if (!room.Terrain[nIdx].CanMoveTo())
                {
                    continue;
                }

                visited[nIdx] = true;
                queue.Enqueue(new Point((byte)nx, (byte)ny));
            }
        }

        // Every walkable tile must have been visited.
        for (byte y = 0; y < room.Height; y++)
        {
            for (byte x = 0; x < room.Width; x++)
            {
                var idx = room.PointToIndex![x, y];
                if (room.Terrain[idx].CanMoveTo() && !visited[idx])
                {
                    Assert.Fail(
                        $"{layout.GetType().Name} seed={seed}: walkable tile ({x},{y}) in room "
                        + $"({room.GridX},{room.GridY}) is unreachable from any connection / start.\n"
                        + DumpRoom(room, visited, starts));
                }
            }
        }
    }

    /// <summary>
    ///     Renders the room's terrain as ASCII. Walkable but unvisited tiles render as
    ///     <c>!</c> so a failing assertion shows exactly where the disconnect is.
    /// </summary>
    private static string DumpRoom(RoomComponent room, bool[] visited, IReadOnlyList<Point> starts)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("Starts: ").AppendJoin(", ", starts.Select(p => $"({p.X},{p.Y})")).Append('\n');
        for (byte y = 0; y < room.Height; y++)
        {
            for (byte x = 0; x < room.Width; x++)
            {
                var idx = room.PointToIndex![x, y];
                var feature = room.Terrain[idx];
                var glyph = feature switch
                {
                    MapFeature.Default => ' ',
                    MapFeature.RockFloor => '.',
                    MapFeature.StoneFloor => ',',
                    MapFeature.RockWall => '#',
                    MapFeature.StoneWall => '%',
                    MapFeature.StoneArchway => '+',
                    _ => '?'
                };
                if (feature.CanMoveTo() && !visited[idx])
                {
                    glyph = '!';
                }
                sb.Append(glyph);
            }
            sb.Append('\n');
        }
        return sb.ToString();
    }
}
