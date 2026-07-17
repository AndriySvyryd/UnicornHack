using UnicornHack.Hubs.ChangeTracking;

namespace UnicornHack.DiagnosticClient.State;

/// <summary>
///     Canonical, mutable client-side mirror of the player's game state.
///     Populated by <see cref="StateApplier" /> from initial snapshots and incremental
///     change sets. All MCP read tools query this single instance.
///     Mutations are guarded by <see cref="Sync" /> so reads from MCP tools are consistent.
/// </summary>
public sealed class GameStateStore
{
    public Lock Sync { get; } = new();

    public PlayerChange? Player { get; private set; }

    /// <summary>Last applied <c>CurrentTick</c> from the server. -1 before any state.</summary>
    public int LastTick { get; private set; } = -1;

    /// <summary>Highest log entry id observed; used by <c>get_log(since_tick)</c>.</summary>
    public int LastObservedLogId { get; private set; } = -1;

    /// <summary>
    ///     Strongly-typed event DTOs from the most recent <c>ReceiveChangeSets</c> batch.
    ///     Cleared at the start of every batch. Each element is a server-side event DTO
    ///     (e.g. <see cref="SteppedOnConnectionEvent" />) decoded from the wire format's
    ///     typeless <see cref="object" /> array via <see cref="EventDecoder" />.
    /// </summary>
    public IReadOnlyList<object> LastEvents { get; private set; } = Array.Empty<object>();

    /// <summary>
    ///     Message of the most recent change-set application failure, or <c>null</c> if the last
    ///     batch applied cleanly. Surfaced by <c>get_diagnostics</c> so an agent driving the
    ///     client can observe protocol violations (e.g. an orphan modify delta) that would
    ///     otherwise only reach stderr. Cleared on the next clean snapshot or change-set batch.
    /// </summary>
    public string? LastApplyError { get; private set; }

    public void ApplyInitialSnapshot(PlayerChange snapshot)
    {
        lock (Sync)
        {
            Player = StateApplier.AdoptFullSnapshot(snapshot);
            LastTick = Player.CurrentTick;
            LastObservedLogId = Player.Log is { Count: > 0 } log ? MaxKey(log) : -1;
            LastEvents = Array.Empty<object>();
            LastApplyError = null;
        }
    }

    public void ApplyChangeSets(IReadOnlyList<TurnChangeSet> sets)
    {
        lock (Sync)
        {
            if (Player is null)
            {
                return;
            }

            List<object>? events = null;
            try
            {
                foreach (var set in sets)
                {
                    StateApplier.ApplyDelta(Player, set.PlayerState);
                    if (set.Events is not { Count: > 0 } rawEvents)
                    {
                        continue;
                    }
                    foreach (var raw in rawEvents)
                    {
                        if (EventDecoder.TryDecode(raw) is { } decoded)
                        {
                            (events ??= []).Add(decoded);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LastApplyError = $"tick {Player.CurrentTick}: {ex.Message}";
                throw;
            }

            LastApplyError = null;
            LastEvents = (IReadOnlyList<object>?)events ?? Array.Empty<object>();

            LastTick = Player.CurrentTick;
            if (Player.Log is { Count: > 0 } log)
            {
                var max = MaxKey(log);
                if (max > LastObservedLogId)
                {
                    LastObservedLogId = max;
                }
            }
        }
    }

    private static int MaxKey<T>(Dictionary<int, T> dict)
    {
        var max = -1;
        foreach (var k in dict.Keys)
        {
            if (k > max)
            {
                max = k;
            }
        }
        return max;
    }
}
