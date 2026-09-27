using System.ComponentModel;
using ModelContextProtocol.Server;
using UnicornHack.DiagnosticClient.Connection;
using UnicornHack.DiagnosticClient.State;
using UnicornHack.Primitives;
using UnicornHack.Systems.Actors;

namespace UnicornHack.DiagnosticClient.Tools;

[McpServerToolType]
public sealed class ActionTools
{
    private readonly GameConnection _conn;
    private readonly TurnTracker _turnTracker;
    private readonly DiagnosticOptions _options;
    private readonly GameStateStore _store;

    public ActionTools(GameConnection conn, TurnTracker turnTracker, DiagnosticOptions options, GameStateStore store)
    {
        _conn = conn;
        _turnTracker = turnTracker;
        _options = options;
        _store = store;
    }

    [McpServerTool, Description(
        "Submit any action by raw ActorActionType integer plus optional target/target2. " +
        "Waits for the resulting change sets and returns a short status string. " +
        "Use the named helpers (move, wait, pickup, …) when possible.")]
    public async Task<string> perform_action(
        int action_type,
        int? target = null,
        int? target2 = null,
        CancellationToken cancellationToken = default)
    {
        return await Invoke((ActorActionType)action_type, target, target2, cancellationToken).ConfigureAwait(false);
    }

    [McpServerTool, Description("Skip your turn. Same as ActorActionType.Wait.")]
    public Task<string> wait(CancellationToken cancellationToken = default)
        => Invoke(ActorActionType.Wait, null, null, cancellationToken);

    [McpServerTool, Description(
        "Move one step. direction is the Direction enum: 0=East 1=Northeast 2=North 3=Northwest 4=West 5=Southwest 6=South 7=Southeast.")]
    public Task<string> move(int direction, CancellationToken cancellationToken = default)
        => Invoke(ActorActionType.MoveOneCell, direction, null, cancellationToken);

    [McpServerTool, Description("Turn in place without moving (uses ChangeHeading). Direction values as in 'move'.")]
    public Task<string> change_heading(int direction, CancellationToken cancellationToken = default)
        => Invoke(ActorActionType.ChangeHeading, direction, null, cancellationToken);

    [McpServerTool, Description("Pathfind to (x, y) in the current room. Coordinates are byte tile indices (0-based).")]
    public Task<string> move_to(int x, int y, CancellationToken cancellationToken = default)
        => Invoke(ActorActionType.MoveToCell, PackPoint(x, y), null, cancellationToken);

    [McpServerTool, Description(
        "Pick up the item at the current tile. There can be at most one item per cell, so " +
        "the action carries no item id. Optional `slot` targets the ability slot the " +
        "picked-up item's primary ability should land in; omit to let the server auto-pick " +
        "the first free slot.")]
    public Task<string> pickup(int? slot = null, CancellationToken cancellationToken = default)
        => Invoke(ActorActionType.PickupItem, slot, null, cancellationToken);

    [McpServerTool, Description("Drop an item by id onto the current tile.")]
    public Task<string> drop(int item_id, CancellationToken cancellationToken = default)
        => Invoke(ActorActionType.DropItem, item_id, null, cancellationToken);

    [McpServerTool, Description("Equip an item into a given EquipmentSlot (int).")]
    public Task<string> equip(int item_id, int slot, CancellationToken cancellationToken = default)
        => Invoke(ActorActionType.EquipItem, item_id, slot, cancellationToken);

    [McpServerTool, Description("Unequip an item by id.")]
    public Task<string> unequip(int item_id, CancellationToken cancellationToken = default)
        => Invoke(ActorActionType.UnequipItem, item_id, null, cancellationToken);

    [McpServerTool, Description("Bind an ability to a slot. Pass ability_id=null to clear the slot.")]
    public Task<string> set_ability_slot(int slot, int? ability_id, CancellationToken cancellationToken = default)
        => Invoke(ActorActionType.SetAbilitySlot, ability_id, slot, cancellationToken);

    [McpServerTool, Description(
        "Activate the ability in the given slot. Optional target_id is the actor id from get_known_actors. "
        + "Omit target_id for self/untargeted abilities.")]
    public Task<string> use_ability_slot(
        int slot, int? target_id = null, CancellationToken cancellationToken = default)
    {
        int? target = null;
        if (target_id is { } actorId)
        {
            lock (_store.Sync)
            {
                if (_store.Player?.Room?.Actors?.GetValueOrDefault(actorId) is not { } actor)
                {
                    return Task.FromResult($"error: actor {actorId} is not known in the current room");
                }

                target = -actorId;
            }
        }

        return Invoke(ActorActionType.UseAbilitySlot, slot, target, cancellationToken);
    }

    [McpServerTool, Description("Travel to a known target room by id (see get_connections / LevelMap.AvailableTargets).")]
    public Task<string> travel_to_room(int room_id, CancellationToken cancellationToken = default)
        => Invoke(ActorActionType.TravelToRoom, room_id, null, cancellationToken);

    [McpServerTool, Description(
        "Select the indexed option from the current choice stage.")]
    public Task<string> select_player_option(
        int option_index, CancellationToken cancellationToken = default)
        => Invoke(ActorActionType.SelectChoice, option_index, null, cancellationToken);

    private async Task<string> Invoke(ActorActionType type, int? target, int? target2, CancellationToken cancellationToken)
    {
        // Two independent budgets:
        //  - InvokeTimeoutMs caps the server-side hub invocation.
        //  - WaitTimeoutMs caps how long we wait for the post-action ReceiveChangeSets
        //    *after* the hub returned. Sharing one budget (the old behavior) made a slow
        //    invoke leave zero room for the wait, surfacing a misleading "timed out"
        //    error even when the server processed the action fine and the change sets
        //    were already in flight.
        //  The wait timeout is treated as soft: if it expires the action is still
        //  considered to have completed (the state will catch up on the next call).
        try
        {
            using var invokeCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            invokeCts.CancelAfter(_options.ActionTimeoutMs);
            try
            {
                await _conn.PerformActionAsync(type, target, target2, invokeCts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (invokeCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                return $"error: action={type} target={target} target2={target2} server invoke timed out after {_options.ActionTimeoutMs}ms";
            }

            try
            {
                using var waitCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                waitCts.CancelAfter(_options.WaitTimeoutMs);
                await _turnTracker.WaitAsync(_options.WaitTimeoutMs, waitCts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // Soft timeout: the server completed the action; the change set may still arrive
                // before the next tool call. Continue with whatever state we already have.
            }
        }
        catch (Exception ex)
        {
            return FormatError(type, target, target2, ex);
        }
        lock (_store.Sync)
        {
            var p = _store.Player;
            var eventCount = _store.LastEvents.Count;
            if (p is null)
            {
                return $"action={type} target={target} target2={target2} (no state)";
            }
            var canAct = p.PendingOptions == null
                         && p.NextActionTick <= p.CurrentTick;
            return $"action={type} target={target} target2={target2} tick={p.CurrentTick} can_act={canAct} hp={p.Hp}/{p.MaxHp} ep={p.Ep}/{p.MaxEp} events={eventCount}";
        }
    }

    private static string FormatError(ActorActionType type, int? target, int? target2, Exception ex)
    {
        // Strip the SignalR HubException prefix so the underlying server message is the headline.
        var msg = ex.Message;
        const string prefix = "An unexpected error occurred invoking 'PerformAction' on the server. ";
        if (msg.StartsWith(prefix, StringComparison.Ordinal))
        {
            msg = msg[prefix.Length..];
        }
        return $"error: action={type} target={target} target2={target2} :: {ex.GetType().Name}: {msg}";
    }

    private static int PackPoint(int x, int y) => (x & 0xFF) << 8 | (y & 0xFF);
}
