using System.ComponentModel;
using System.Globalization;
using System.Text;
using ModelContextProtocol.Server;
using UnicornHack.DiagnosticClient.Connection;
using UnicornHack.DiagnosticClient.Rendering;
using UnicornHack.DiagnosticClient.State;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Primitives;

namespace UnicornHack.DiagnosticClient.Tools;

[McpServerToolType]
public sealed class ReadTools
{
    private readonly GameStateStore _store;
    private readonly GameConnection _conn;

    public ReadTools(GameStateStore store, GameConnection conn)
    {
        _store = store;
        _conn = conn;
    }

    [McpServerTool, Description(
        "Compact one/two-line status snapshot. Includes HP/EP/XP/tick/can_act so an agent " +
        "can decide what to do next without pulling the full state.")]
    public string get_status()
    {
        lock (_store.Sync)
        {
            var p = _store.Player;
            if (p is null)
            {
                return "(state not yet received)";
            }
            var room = p.Room;
            var canAct = p.NextActionTick <= p.CurrentTick;
            var depth = room?.Depth ?? 0;
            var branch = room?.BranchName ?? "?";
            var pos = "-";
            var head = "-";
            if (room?.Actors is { } actors && actors.TryGetValue(p.Id, out var me))
            {
                pos = $"({me.LevelX},{me.LevelY})";
                head = ((Direction)me.Heading).ToString();
            }
            return $"name={p.Name} id={p.Id} pos={pos} head={head} tick={p.CurrentTick} can_act={canAct} next_at={p.NextActionTick} "
                 + $"hp={p.Hp}/{p.MaxHp} ep={p.Ep}/{p.MaxEp} reserved_ep={p.ReservedEp} xp={p.XP} fortune={p.Fortune} "
                 + $"branch={branch} depth={depth} room={room?.Width}x{room?.Height}@({room?.GridX},{room?.GridY})";
        }
    }

    [McpServerTool, Description(
        "ASCII map of the current room. Pass include_legend=true for the legend line. " +
        "Cells outside currently-visible terrain are blanked out.")]
    public string get_map(bool include_legend = false)
    {
        lock (_store.Sync)
        {
            var p = _store.Player;
            return p is null ? "(state not yet received)" : AsciiMap.Render(p, include_legend);
        }
    }

    [McpServerTool, Description(
        "List actors known to the player in the current room (including the player). By default lists both " +
        "currently-perceived and remembered actors; pass only_perceived=true to filter to live FOV. " +
        "Columns: id symbol name x y dist hp/maxHp perceived heading next_at action.")]
    public string get_known_actors(bool only_perceived = false)
    {
        lock (_store.Sync)
        {
            var p = _store.Player;
            if (p?.Room?.Actors is not { } actors)
            {
                return "(no actor data)";
            }
            var sb = new StringBuilder();
            sb.Append("id\tsym\tname\tx\ty\tdist\thp/max\tperc\thead\tnext\taction\n");
            actors.TryGetValue(p.Id, out var self);
            foreach (var (id, a) in actors.OrderBy(kv => kv.Key))
            {
                if (only_perceived && !a.IsCurrentlyPerceived && id != p.Id)
                {
                    continue;
                }
                var sym = id == p.Id ? "@" : SymFor(a.Name ?? a.BaseName, a.IsCurrentlyPerceived);
                var action = a.NextAction?.Type.ToString() ?? "-";
                var dist = self is null || id == p.Id
                    ? 0
                    : Math.Max(Math.Abs(a.LevelX - self.LevelX), Math.Abs(a.LevelY - self.LevelY));
                sb.Append(id).Append('\t').Append(sym).Append('\t').Append(a.Name ?? a.BaseName ?? "?")
                  .Append('\t').Append(a.LevelX).Append('\t').Append(a.LevelY)
                  .Append('\t').Append(dist)
                  .Append('\t').Append(a.Hp).Append('/').Append(a.MaxHp)
                  .Append('\t').Append(a.IsCurrentlyPerceived ? "Y" : "n")
                  .Append('\t').Append((Direction)a.Heading)
                  .Append('\t').Append(a.NextActionTick)
                  .Append('\t').Append(action).Append('\n');
            }
            return sb.ToString();
        }
    }

    [McpServerTool, Description("List items currently in the room. Columns: id type name x y perceived.")]
    public string get_items()
    {
        lock (_store.Sync)
        {
            var p = _store.Player;
            if (p?.Room?.Items is not { } items)
            {
                return "(no item data)";
            }
            var sb = new StringBuilder();
            sb.Append("id\ttype\tname\tx\ty\tperc\n");
            foreach (var (id, i) in items.OrderBy(kv => kv.Key))
            {
                sb.Append(id).Append('\t').Append(i.Type).Append('\t').Append(i.Name ?? i.BaseName ?? "?")
                  .Append('\t').Append(i.LevelX).Append('\t').Append(i.LevelY)
                  .Append('\t').Append(i.IsCurrentlyPerceived ? "Y" : "n").Append('\n');
            }
            return sb.ToString();
        }
    }

    [McpServerTool,
        Description("List the player's abilities. Columns: id name slot activation cooldown_tick cooldown_xp_left targeting accumulator.")]
    public string get_abilities(bool only_slotted = false)
    {
        lock (_store.Sync)
        {
            var p = _store.Player;
            if (p?.Abilities is not { } abilities)
            {
                return "(no ability data)";
            }
            var sb = new StringBuilder();
            sb.Append("id\tname\tslot\tactivation\tcd_tick\tcd_xp\tshape:size\taccumulator\n");
            foreach (var (id, a) in abilities.OrderBy(kv => kv.Key))
            {
                if (only_slotted && a.Slot is null)
                {
                    continue;
                }
                sb.Append(id).Append('\t').Append(a.Name ?? "?")
                  .Append('\t').Append(a.Slot?.ToString(CultureInfo.InvariantCulture) ?? "-")
                  .Append('\t').Append(a.Activation)
                  .Append('\t').Append(a.CooldownTick?.ToString(CultureInfo.InvariantCulture) ?? "-")
                  .Append('\t').Append(a.CooldownXpLeft?.ToString(CultureInfo.InvariantCulture) ?? "-")
                  .Append('\t').Append(a.TargetingShape).Append(':').Append(a.TargetingShapeSize)
                  .Append('\t').Append(a.Accumulator?.ToString(CultureInfo.InvariantCulture) ?? "-").Append('\n');
            }
            return sb.ToString();
        }
    }

    [McpServerTool, Description("List the player's race abilities. Columns: id name level.")]
    public string get_races()
    {
        lock (_store.Sync)
        {
            var p = _store.Player;
            if (p?.Abilities is not { } abilities)
            {
                return "(no race data)";
            }
            var sb = new StringBuilder();
            sb.Append("id\tname\tlevel\n");
            foreach (var (id, ability) in abilities
                         .Where(kv => kv.Value.Type == (int)AbilityType.Race)
                         .OrderBy(kv => kv.Key))
            {
                sb.Append(id).Append('\t').Append(ability.Name ?? "?").Append('\t').Append(ability.Level).Append('\n');
            }
            return sb.ToString();
        }
    }

    [McpServerTool, Description("List intra-room connections (stairs/doors). Columns: id x y orientation.")]
    public string get_connections()
    {
        lock (_store.Sync)
        {
            var p = _store.Player;
            if (p?.Room?.Connections is not { } conns)
            {
                return "(no connection data)";
            }
            var sb = new StringBuilder();
            sb.Append("id\tx\ty\torient\n");
            foreach (var (id, c) in conns.OrderBy(kv => kv.Key))
            {
                sb.Append(id).Append('\t').Append(c.LevelX).Append('\t').Append(c.LevelY)
                  .Append('\t').Append(c.Orientation).Append('\n');
            }
            return sb.ToString();
        }
    }

    [McpServerTool, Description(
        "Returns recent log entries. Defaults to the last 20 entries. Pass since_tick to get only entries " +
        "with Ticks > since_tick (overrides last_n). When strip_tags is true (default) inline markup like " +
        "<damage physical='68'>68</damage> is stripped to just the inner value; pass false to keep raw markup. " +
        "Use this after each action to learn what happened.")]
    public string get_log(int last_n = 20, int since_tick = -1, bool strip_tags = true)
    {
        lock (_store.Sync)
        {
            var p = _store.Player;
            if (p?.Log is not { } log)
            {
                return "(no log data)";
            }
            var ordered = log.OrderBy(kv => kv.Key).ToList();
            IEnumerable<KeyValuePair<int, LogEntryChange>> selected;
            if (since_tick >= 0)
            {
                selected = ordered.Where(kv => kv.Value.Ticks > since_tick);
            }
            else
            {
                var take = Math.Max(0, last_n);
                selected = take >= ordered.Count ? ordered : ordered.GetRange(ordered.Count - take, take);
            }
            var sb = new StringBuilder();
            foreach (var (_, entry) in selected)
            {
                var message = strip_tags ? StripLogMarkup(entry.Message) : entry.Message ?? string.Empty;
                sb.Append('[').Append(entry.Ticks).Append("] ").Append(message).Append('\n');
            }
            if (sb.Length == 0)
            {
                sb.Append("(no log entries)\n");
            }
            return sb.ToString();
        }
    }

    private static string StripLogMarkup(string? message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return string.Empty;
        }
        // Replace <tag ...>inner</tag> with just `inner`. The server emits markup like
        // <damage physical='68'>68</damage> for color highlighting; we keep the value only.
        var sb = new StringBuilder(message.Length);
        var i = 0;
        while (i < message.Length)
        {
            var c = message[i];
            if (c == '<')
            {
                var close = message.IndexOf('>', i + 1);
                if (close < 0) { sb.Append(message, i, message.Length - i); break; }
                i = close + 1;
                continue;
            }
            sb.Append(c);
            i++;
        }
        return sb.ToString();
    }

    [McpServerTool, Description("Last DialogData/QueryGame payload (list-of-objects) received from ReceiveUIRequest or the most recent show_* call. Returns (empty) if no UI request pending.")]
    public string get_dialog()
        => DialogPayloadFormatter.Render(_conn.LastUIRequest);

    [McpServerTool, Description("Recent chat messages (oldest first). Columns: user message.")]
    public string get_chat()
    {
        var sb = new StringBuilder();
        foreach (var (user, message) in _conn.Chat)
        {
            sb.Append(user).Append(": ").Append(message).Append('\n');
        }
        return sb.Length == 0 ? "(no chat)" : sb.ToString();
    }

    [McpServerTool, Description(
        "Strongly-typed events recorded from the most recent action's ReceiveChangeSets batch. " +
        "Each line is one event in the form '<EventName> field1=value1 field2=value2 …'. " +
        "Cleared and repopulated on every action that produces change sets.")]
    public string get_events()
    {
        lock (_store.Sync)
        {
            var events = _store.LastEvents;
            if (events.Count == 0)
            {
                return "(no events)";
            }
            var sb = new StringBuilder();
            foreach (var ev in events)
            {
                switch (ev.Type)
                {
                    case TurnEventType.SteppedOnConnection:
                        sb.Append("SteppedOnConnection scope=").Append((ConnectionScope)ev.Value1)
                            .Append(" connection_id=").Append(ev.Value2).Append('\n');
                        break;
                    default:
                        sb.Append(ev.Type).Append(" value1=").Append(ev.Value1)
                            .Append(" value2=").Append(ev.Value2).Append('\n');
                        break;
                }
            }
            return sb.ToString();
        }
    }

    [McpServerTool, Description("Connection diagnostics: hub state, last applied tick, last log id, options.")]
    public string get_diagnostics()
    {
        lock (_store.Sync)
        {
              var abilities = _store.Player?.Abilities;
              var raceCount = abilities?.Values.Count(a => a.Type == (int)AbilityType.Race) ?? 0;
              return $"hub={_conn.ConnectionState} state_ready={_conn.StateReady} last_tick={_store.LastTick} last_log_id={_store.LastObservedLogId} "
                  + $"races={raceCount} abilities={abilities?.Count ?? 0} "
                  + $"effects={_store.Player?.Effects?.Count ?? 0} "
                 + $"actors={_store.Player?.Room?.Actors?.Count ?? 0} items={_store.Player?.Room?.Items?.Count ?? 0} "
                 + $"events={_store.LastEvents.Count}"
                 + (_store.LastApplyError is { } err ? $" apply_error=\"{err}\"" : "");
        }
    }

    private static string SymFor(string? name, bool perceived)
    {
        if (string.IsNullOrEmpty(name))
        {
            return "?";
        }
        var ch = char.IsLetter(name[0]) ? name[0] : '?';
        return (perceived ? char.ToUpperInvariant(ch) : char.ToLowerInvariant(ch)).ToString();
    }

    [McpServerTool, Description(
        "ASCII visibility map of the current room. Each currently-perceived cell renders as a digit 0-9 " +
        "corresponding to bucketed VisibleTerrain intensity (0=very dim, 9=brightest). Non-perceived cells " +
        "render as a space. The player is shown as '@'. Useful for inspecting FOV cone shape and falloff.")]
    public string get_visibility_map()
    {
        lock (_store.Sync)
        {
            var p = _store.Player;
            return p is null ? "(state not yet received)" : AsciiMap.RenderVisibility(p);
        }
    }

    [McpServerTool, Description(
        "Debug: dumps raw LevelMap.Terrain (T) and VisibleTerrain (V) byte values for a rectangular region. " +
        "Columns are x; rows are y. Each cell is rendered as T/V (decimal). Use to diagnose renderer/terrain mismatches.")]
    public string get_terrain_raw(int x0, int y0, int width, int height)
    {
        lock (_store.Sync)
        {
            var p = _store.Player;
            if (p?.Room is not { } room || room.LevelMap is not { } map)
            {
                return "(no map data)";
            }
            var w = room.Width;
            var h = room.Height;
            var sb = new StringBuilder();
            sb.Append("region (").Append(x0).Append(',').Append(y0).Append(") size ")
              .Append(width).Append('x').Append(height).Append(" — cell = Terrain/Visible\n");
            sb.Append("    ");
            for (var x = x0; x < x0 + width && x < w; x++)
            {
                sb.Append($"{x,5} ");
            }
            sb.Append('\n');
            for (var y = y0; y < y0 + height && y < h; y++)
            {
                sb.Append($"{y,3}: ");
                for (var x = x0; x < x0 + width && x < w; x++)
                {
                    if (x < 0 || y < 0)
                    {
                        sb.Append("  -/- ");
                        continue;
                    }
                    var idx = y * w + x;
                    var t = (byte)map.Terrain[idx];
                    var v = map.VisibleTerrain[idx];
                    sb.Append($"{t,3}/{v,-3} ");
                }
                sb.Append('\n');
            }
            return sb.ToString();
        }
    }
}
