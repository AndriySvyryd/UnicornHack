using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Hubs.ChangeTracking;

/// <summary>
///     Per-turn collector that translates Core-side messages into transport events
///     attached to <see cref="TurnChangeSet.Events" />. Currently handles
///     <see cref="SteppedOnConnectionMessage" /> for player actors; the message queue
///     drops events for non-players or when no observer is registered.
/// </summary>
/// <remarks>
///     Each pending event is a strongly-typed DTO whose <c>[Key(0)]</c> is the
///     <see cref="TurnEventType" /> discriminator — the client reads index 0 of the
///     incoming MessagePack array to dispatch on event kind. Events are keyed by the
///     player entity id so <see cref="GameStateManager.Turn" /> can attach them to the
///     right player <see cref="TurnChangeSet" />.
/// </remarks>
public class EventBuilder : IGameSystem<SteppedOnConnectionMessage>
{
    private readonly Dictionary<int, List<object?>> _pendingByPlayer = new();

    public MessageProcessingResult Process(SteppedOnConnectionMessage message, GameManager manager)
    {
        var actor = message.ActorEntity;
        if (!actor.HasComponent(EntityComponent.Player))
        {
            return MessageProcessingResult.ContinueProcessing;
        }

        var connection = message.ConnectionEntity.Connection!;
        var sourceLevel = message.ConnectionEntity.Position!.RoomEntity.Room!.Level;
        var targetRoom = connection.TargetRoomEntity.Room;
        var targetLevel = targetRoom?.Level;

        ConnectionScope scope;
        if (targetLevel == null || targetLevel == sourceLevel)
        {
            scope = ConnectionScope.Room;
        }
        else if (targetLevel.Branch == sourceLevel.Branch)
        {
            scope = ConnectionScope.Level;
        }
        else
        {
            scope = ConnectionScope.Branch;
        }

        if (!_pendingByPlayer.TryGetValue(actor.Id, out var list))
        {
            list = [];
            _pendingByPlayer[actor.Id] = list;
        }

        list.Add(new SteppedOnConnectionEvent
        {
            Scope = scope,
            ConnectionId = message.ConnectionEntity.Id
        });

        return MessageProcessingResult.ContinueProcessing;
    }

    /// <summary>
    ///     Returns the events recorded for the given player since the last
    ///     <see cref="Clear" />, or <c>null</c> if there are none.
    /// </summary>
    public List<object?>? GetEvents(int playerEntityId)
        => _pendingByPlayer.TryGetValue(playerEntityId, out var list) && list.Count > 0
            ? list
            : null;

    public void Clear()
        => _pendingByPlayer.Clear();

    public void RegisterOnGroups(GameManager manager)
        => manager.Queue.Register<SteppedOnConnectionMessage>(
            this, SteppedOnConnectionMessage.Name, order: 0);
}
