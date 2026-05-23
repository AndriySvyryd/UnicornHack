using System.Text;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Primitives;

namespace UnicornHack.DiagnosticClient.Rendering;

/// <summary>
///     Renders the current room to a compact ASCII grid, overlaying actors, items,
///     and connections on top of terrain. Designed for token-efficient consumption
///     by an LLM agent.
///     Legend (rows top-to-bottom = increasing Y):
///       Terrain:  ' '=unexplored '.'=floor '#'=wall '*'=archway '~'=pool '?'=other
///       Overlay:  '@'=you  uppercase letter = perceived hostile/NPC actor
///                 lowercase letter = remembered (not currently perceived) actor
///                 '$'=item  '&gt;'/'&lt;'=connection
/// </summary>
public static class AsciiMap
{
    public static string Render(PlayerChange player, bool includeLegend)
    {
        if (player.Room is not { } room || room.LevelMap is not { } map)
        {
            return "(no room data)";
        }

        var w = room.Width;
        var h = room.Height;
        if (w <= 0 || h <= 0 || map.Terrain.Length < w * h)
        {
            return $"(room dimensions {w}x{h} inconsistent with terrain length {map.Terrain.Length})";
        }

        var grid = new char[h, w];
        var hasVisibility = map.VisibleTerrain.Length >= w * h;

        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                var idx = y * w + x;
                var feature = map.Terrain[idx];
                if (feature == MapFeature.Unexplored)
                {
                    grid[y, x] = ' ';
                    continue;
                }
                var inFov = !hasVisibility || map.VisibleTerrain[idx] != 0;
                grid[y, x] = inFov ? FeatureChar(feature) : RememberedFeatureChar(feature);
            }
        }

        // Connections
        if (room.Connections is { } conns)
        {
            foreach (var c in conns.Values)
            {
                Put(grid, c.LevelX, c.LevelY, w, h, ConnectionChar(c.Orientation));
            }
        }

        // Items
        if (room.Items is { } items)
        {
            foreach (var i in items.Values)
            {
                if (i.IsCurrentlyPerceived)
                {
                    Put(grid, (byte)i.LevelX, (byte)i.LevelY, w, h, '$');
                }
            }
        }

        // Actors (player wins ties)
        if (room.Actors is { } actors)
        {
            foreach (var (id, a) in actors)
            {
                if (id == player.Id)
                {
                    continue;
                }
                var name = a.Name ?? a.BaseName ?? "?";
                var ch = char.IsLetter(name[0]) ? name[0] : '?';
                ch = a.IsCurrentlyPerceived ? char.ToUpperInvariant(ch) : char.ToLowerInvariant(ch);
                Put(grid, a.LevelX, a.LevelY, w, h, ch);
            }

            if (actors.TryGetValue(player.Id, out var me))
            {
                Put(grid, me.LevelX, me.LevelY, w, h, '@');
            }
        }

        var sb = new StringBuilder((w + 1) * h + 64);
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                sb.Append(grid[y, x]);
            }
            sb.Append('\n');
        }
        if (includeLegend)
        {
            sb.Append("legend: @=you  A-Z=visible actor  a-z=remembered  $=item  >/<=stairs |/-=doorway  ");
            sb.Append(".=floor  #=wall  *=arch  ~=pool  ,=remembered floor  %=remembered wall  ?=unidentified\n");
        }
        return sb.ToString();
    }

    private static void Put(char[,] grid, byte x, byte y, int w, int h, char ch)
    {
        if (x < w && y < h)
        {
            grid[y, x] = ch;
        }
    }
    private static void Put(char[,] grid, int x, int y, int w, int h, char ch)
    {
        if (x >= 0 && x < w && y >= 0 && y < h)
        {
            grid[y, x] = ch;
        }
    }

    private static char FeatureChar(MapFeature f) => f switch
    {
        MapFeature.Default or MapFeature.Unexplored => ' ',
        MapFeature.RockFloor or MapFeature.StoneFloor => '.',
        MapFeature.RockWall or MapFeature.StoneWall => '#',
        MapFeature.StoneArchway => '*',
        MapFeature.Pool => '~',
        _ => '?'
    };

    private static char RememberedFeatureChar(MapFeature f) => f switch
    {
        MapFeature.Default or MapFeature.Unexplored => ' ',
        MapFeature.RockFloor or MapFeature.StoneFloor => ',',
        MapFeature.RockWall or MapFeature.StoneWall => '%',
        MapFeature.StoneArchway => '=',
        MapFeature.Pool => ':',
        _ => '?'
    };

    /// <summary>
    ///     Renders one digit per cell representing visibility intensity (VisibleTerrain byte
    ///     bucketed into 0-9). Cells outside the current FOV render as ' '. The player is shown
    ///     as '@'. Useful for diagnosing FOV cone shape, falloff and cells that are nominally
    ///     visible but at very low intensity.
    /// </summary>
    public static string RenderVisibility(PlayerChange player)
    {
        if (player.Room is not { } room || room.LevelMap is not { } map)
        {
            return "(no room data)";
        }

        var w = room.Width;
        var h = room.Height;
        if (w <= 0 || h <= 0 || map.VisibleTerrain.Length < w * h)
        {
            return $"(room dimensions {w}x{h} inconsistent with visibility length {map.VisibleTerrain.Length})";
        }

        var grid = new char[h, w];
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                var v = map.VisibleTerrain[y * w + x];
                grid[y, x] = v == 0 ? ' ' : (char)('0' + Math.Min(9, v * 10 / 256));
            }
        }

        byte? px = null, py = null;
        if (room.Actors is { } actors && actors.TryGetValue(player.Id, out var me))
        {
            px = me.LevelX;
            py = me.LevelY;
        }

        var sb = new StringBuilder((w + 1) * h + 32);
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                if (px == x && py == y)
                {
                    sb.Append('@');
                }
                else
                {
                    sb.Append(grid[y, x]);
                }
            }
            sb.Append('\n');
        }
        return sb.ToString();
    }

    private static char ConnectionChar(Direction d) => d switch
    {
        Direction.Up => '<',
        Direction.Down => '>',
        Direction.North or Direction.South => '|',
        Direction.East or Direction.West => '-',
        Direction.Northeast or Direction.Southwest => '/',
        Direction.Northwest or Direction.Southeast => '\\',
        _ => '+'
    };
}
