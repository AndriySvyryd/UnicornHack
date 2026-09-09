using UnicornHack.Services;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Hubs.ChangeTracking;

public class RoomChangeBuilder
{
    private readonly ActorChangeBuilder _actorBuilder = new();
    private readonly ItemChangeBuilder _itemBuilder = new();
    private readonly ConnectionChangeBuilder _connectionBuilder = new();
    private readonly EventBuilder _eventBuilder = new();

    // Item, connection, and terrain DTOs are observer-independent for players sharing a room.
    private RoomChange? _serializedCommonRoom;
    private bool _commonRoomSerialized;

    public List<PlayerChangeBuilder> PlayerBuilders { get; } = [];

    public EventBuilder EventBuilder => _eventBuilder;

    public bool HasChanges(RoomComponent? room, in SerializationContext context)
    {
        if (room == null)
        {
            return false;
        }

        if (_commonRoomSerialized && _serializedCommonRoom != null)
        {
            return true;
        }

        var hasActorChanges = _actorBuilder.HasChangesForRoom(room, context.Services);
        var hasItemChanges = _itemBuilder.HasChangesForRoom(room.EntityId, room.Entity.Game.Services);
        var hasConnectionChanges = _connectionBuilder.HasChangesForRoom(room.EntityId);

        return hasActorChanges || hasItemChanges || hasConnectionChanges
               || TerrainChangeBuilder.HasChanges(room);
    }

    public RoomChange? GetSerializedRoom(
        GameEntity? roomEntity, in SerializationContext context)
    {
        var room = roomEntity?.Room;
        if (room == null)
        {
            return null;
        }

        PrepareCommonRoom(roomEntity, context.Services);
        return _serializedCommonRoom;
    }

    public void PrepareCommonRoom(GameEntity? roomEntity, GameServices services)
    {
        if (!_commonRoomSerialized)
        {
            var room = roomEntity?.Room;
            if (room == null)
            {
                _commonRoomSerialized = true;
                return;
            }

            var commonChange = new RoomChange();
            var hasCommonChanges = false;

            var commonActorChanges = _actorBuilder.BuildCommonChanges(room, services);
            if (commonActorChanges != null)
            {
                commonChange.Actors = commonActorChanges;
                hasCommonChanges = true;
            }

            if (_itemBuilder.HasChangesForRoom(room.Entity.Id, services))
            {
                var serializedItems = _itemBuilder.BuildChanges(room.Entity.Id, services);
                if (serializedItems != null)
                {
                    commonChange.Items = serializedItems;
                    hasCommonChanges = true;
                }
            }

            if (_connectionBuilder.HasChangesForRoom(room.Entity.Id))
            {
                var serializedConnections = _connectionBuilder.BuildChanges(room.Entity.Id);
                if (serializedConnections != null)
                {
                    commonChange.Connections = serializedConnections;
                    hasCommonChanges = true;
                }
            }

            if (TerrainChangeBuilder.HasChanges(room))
            {
                TerrainChangeBuilder.WriteToRoomChange(room, commonChange);
                hasCommonChanges = true;
            }

            _serializedCommonRoom = hasCommonChanges ? commonChange : null;
            _commonRoomSerialized = true;
        }
    }

    /// <summary>
    ///     Discards every accumulated per-turn delta without rebuilding persistent
    ///     client-known projections.
    ///     <para>
    ///         Must not be called from a read endpoint (e.g. <c>GetState</c>) — multiple
    ///         clients may share a builder, so a read that advanced the log high-water
    ///         mark would skip entries for the other clients.
    ///     </para>
    /// </summary>
    public void Reset(GameManager manager)
    {
        _actorBuilder.Reset();
        _itemBuilder.Reset();
        _connectionBuilder.Reset();
        _eventBuilder.Reset();
        _serializedCommonRoom = null;
        _commonRoomSerialized = false;

        foreach (var builder in PlayerBuilders)
        {
            var playerEntity = manager.FindEntity(builder.PlayerEntityId)!;
            builder.Reset(playerEntity);
        }
    }

    public static RoomChange SerializeRoom(
        GameEntity roomEntity, in SerializationContext context, RoomChangeBuilder? builder = null)
    {
        var room = roomEntity.Room!;
        if (builder?._serializedCommonRoom is not { ChangedProperties: null } common
            || common.BranchName != room.Level.BranchName
            || common.Depth != room.Level.Depth
            || common.GridX != room.GridX
            || common.GridY != room.GridY)
        {
            common = SerializeFullRoomCommon(roomEntity, in context);
            if (builder != null)
            {
                builder._serializedCommonRoom = common;
            }
        }

        return ComposeFullRoom(
            common,
            SerializeActors(room, in context));
    }

    private static Dictionary<int, ActorChange> SerializeActors(
        RoomComponent room, in SerializationContext context)
    {
        var actors = new Dictionary<int, ActorChange>(room.KnownActors.Count);
        var canPerceiveIntentions = ActorChangeBuilder.CanPerceiveIntentions(room);
        foreach (var actorPosition in room.KnownActors.Values)
        {
            var actor = actorPosition.Knowledge!.Entity;
            actors.Add(
                actor.Id,
                ActorChangeBuilder.SerializeActor(actor, in context, canPerceiveIntentions));
        }

        return actors;
    }

    private static RoomChange SerializeFullRoomCommon(
        GameEntity roomEntity, in SerializationContext context)
    {
        var room = roomEntity.Room!;

        var items = new Dictionary<int, ItemChange>(room.KnownItems.Count);
        foreach (var itemPosition in room.KnownItems.Values)
        {
            var item = itemPosition.Knowledge!.Entity;
            items.Add(item.Id, ItemChangeBuilder.SerializeItem(item, in context));
        }

        var connectionCount = 0;
        foreach (var connectionPosition in room.KnownConnections.Values)
        {
            var directionality = connectionPosition.Knowledge!.KnownEntity.Connection!.Directionality;
            if (directionality == null || (directionality & Directionality.Source) != 0)
            {
                connectionCount++;
            }
        }

        var connections = new Dictionary<int, ConnectionChange>(connectionCount);
        foreach (var connectionPosition in room.KnownConnections.Values)
        {
            var knowledge = connectionPosition.Knowledge!;
            var directionality = knowledge.KnownEntity.Connection!.Directionality;
            if (directionality != null && (directionality & Directionality.Source) == 0)
            {
                continue;
            }

            var connection = knowledge.Entity;
            var connectionChange = ConnectionChangeBuilder.SerializeConnection(connection);
            connections.Add(connection.Id, connectionChange);
        }

        var change = new RoomChange
        {
            ChangedProperties = null,
            Items = items,
            Connections = connections,
            BranchName = room.Level.BranchName,
            Depth = room.Level.Depth,
            Width = room.Width,
            Height = room.Height,
            GridX = room.GridX,
            GridY = room.GridY
        };

        SerializeMap(room, change);
        return change;
    }

    private static RoomChange ComposeFullRoom(
        RoomChange common, Dictionary<int, ActorChange> actors)
        => new()
        {
            ChangedProperties = null,
            Actors = actors,
            Items = common.Items,
            Connections = common.Connections,
            LevelMap = common.LevelMap,
            LevelMapChanges = common.LevelMapChanges,
            BranchName = common.BranchName,
            Depth = common.Depth,
            Width = common.Width,
            Height = common.Height,
            GridX = common.GridX,
            GridY = common.GridY
        };

    private static void SerializeMap(RoomComponent room, RoomChange change)
        => TerrainChangeBuilder.WriteSnapshotToRoomChange(room, change);

    public void RegisterOnGroups(GameManager manager)
    {
        _actorBuilder.RegisterOnGroups(manager);
        _itemBuilder.RegisterOnGroups(manager);
        _connectionBuilder.RegisterOnGroups(manager);
        _eventBuilder.RegisterOnGroups(manager);

        foreach (var playerEntity in manager.Players)
        {
            RegisterPlayerBuilder(playerEntity, manager);
        }
    }

    /// <summary>
    ///     Ensures a <see cref="PlayerChangeBuilder" /> is registered for
    ///     <paramref name="playerEntity" />. When no builder exists for the player's id,
    ///     a fresh one is constructed and wired onto <paramref name="manager" />'s groups.
    ///     When one already exists, its baseline is realigned against
    ///     <paramref name="playerEntity" /> via <see cref="PlayerChangeBuilder.Reset" /> —
    ///     the same alignment <see cref="Reset" /> performs on every registered builder.
    ///     This makes the call idempotent in a "freshly registered" sense: after it
    ///     returns, the matching builder's "what the client knows" baseline matches the
    ///     current world state regardless of whether it was created here or earlier.
    /// </summary>
    public void RegisterPlayerBuilder(GameEntity playerEntity, GameManager manager)
    {
        _actorBuilder.ResetProjection(playerEntity);
        _itemBuilder.ResetProjection(playerEntity);
        _connectionBuilder.ResetProjection(playerEntity);
        foreach (var existing in PlayerBuilders)
        {
            if (existing.PlayerEntityId == playerEntity.Id)
            {
                existing.ResetProjection(playerEntity);
                return;
            }
        }

        var builder = new PlayerChangeBuilder(playerEntity, this);
        builder.RegisterOnGroups(manager);
        PlayerBuilders.Add(builder);
    }

    internal void ResetRoomBaseline(GameEntity playerEntity)
    {
        _actorBuilder.ResetProjection(playerEntity);
        _itemBuilder.ResetProjection(playerEntity);
        _connectionBuilder.ResetProjection(playerEntity);
    }

    public void UnregisterFromGroups(GameManager manager)
    {
        _actorBuilder.UnregisterFromGroups(manager);
        _itemBuilder.UnregisterFromGroups(manager);
        _connectionBuilder.UnregisterFromGroups(manager);

        foreach (var builder in PlayerBuilders)
        {
            builder.UnregisterFromGroups(manager);
        }
    }

}
