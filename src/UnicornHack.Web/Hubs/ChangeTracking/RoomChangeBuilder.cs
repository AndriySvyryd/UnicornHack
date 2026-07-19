using UnicornHack.Services;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Hubs.ChangeTracking;

public class RoomChangeBuilder
{
    private readonly ActorChangeBuilder _actorBuilder = new();
    private readonly ItemChangeBuilder _itemBuilder = new();
    private readonly ConnectionChangeBuilder _connectionBuilder = new();
    private readonly EventBuilder _eventBuilder = new();

    public List<PlayerChangeBuilder> PlayerBuilders { get; } = [];

    public EventBuilder EventBuilder => _eventBuilder;

    public bool HasChanges(RoomComponent? room)
    {
        if (room == null)
        {
            return false;
        }

        var manager = room.Entity.Manager;
        var roomId = room.Entity.Id;

        // Prune cross-room entries from every builder (don't short-circuit — all three must be
        // visited). HasObservableChanges always calls it before
        // BuildChangeSet, so GetSerializedRoom can serialize the already-scoped builders directly.
        var hasActorChanges = _actorBuilder.RemoveChangesNotInRoom(manager, roomId);
        var hasItemChanges = _itemBuilder.RemoveChangesNotInRoom(manager, roomId);
        var hasConnectionChanges = _connectionBuilder.RemoveChangesNotInRoom(manager, roomId);

        return hasActorChanges || hasItemChanges || hasConnectionChanges
               || TerrainChangeBuilder.HasChanges(room);
    }

    public RoomChange? GetSerializedRoom(
        GameEntity? roomEntity, GameEntity observer, GameServices services)
    {
        var room = roomEntity?.Room;
        if (room == null)
        {
            return null;
        }

        // Cross-room entries were already pruned by HasObservableChanges (which always runs and
        // prunes before this is reached), so the builders hold only this room's entries.
        var context = new SerializationContext(null!, observer, services);
        var change = new RoomChange();
        var hasChanges = false;

        if (_actorBuilder.HasChanges)
        {
            var serializedActors = new Dictionary<int, ActorChange>();
            _actorBuilder.WriteTo(serializedActors, context);
            if (serializedActors.Count > 0)
            {
                change.Actors = serializedActors;
                hasChanges = true;
            }
        }

        if (_itemBuilder.HasChanges)
        {
            var serializedItems = new Dictionary<int, ItemChange>();
            _itemBuilder.WriteTo(serializedItems, context);
            if (serializedItems.Count > 0)
            {
                change.Items = serializedItems;
                hasChanges = true;
            }
        }

        if (_connectionBuilder.HasChanges)
        {
            var serializedConnections = new Dictionary<int, ConnectionChange>();
            _connectionBuilder.WriteTo(serializedConnections, context);
            if (serializedConnections.Count > 0)
            {
                change.Connections = serializedConnections;
                hasChanges = true;
            }
        }

        if (TerrainChangeBuilder.HasChanges(room))
        {
            TerrainChangeBuilder.WriteToRoomChange(room, change);
            hasChanges = true;
        }

        return hasChanges ? change : null;
    }

    /// <summary>
    ///     Discards every accumulated per-turn delta (actor / item / connection diffs and
    ///     buffered events) and realigns each registered <see cref="PlayerBuilders" />
    ///     entry's baseline against the current world state via
    ///     <see cref="PlayerChangeBuilder.Reset" />.
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

        foreach (var builder in PlayerBuilders)
        {
            var playerEntity = manager.FindEntity(builder.PlayerEntityId)!;
            builder.Reset(playerEntity);
        }
    }

    public static RoomChange SerializeRoom(
        GameEntity roomEntity, SerializationContext context)
    {
        var room = roomEntity.Room!;

        var actors = new Dictionary<int, ActorChange>();
        foreach (var actor in room.KnownActors.Values
                     .Select(a => a.Knowledge!).Select(a => a.Entity))
        {
            actors.Add(actor.Id, ActorChangeBuilder.SerializeActor(actor, context));
        }

        var items = new Dictionary<int, ItemChange>();
        foreach (var item in room.KnownItems.Values
                     .Select(t => t.Knowledge!).Select(t => t.Entity))
        {
            items.Add(item.Id, ItemChangeBuilder.SerializeItem(item, context));
        }

        var connections = new Dictionary<int, ConnectionChange>();
        foreach (var connection in room.KnownConnections.Values
                     .Select(c => c.Knowledge!)
                     .Where(c => c.KnownEntity.Connection!.Directionality == null
                                 || (c.KnownEntity.Connection.Directionality & Directionality.Source) != 0)
                     .Select(c => c.Entity))
        {
            var connectionChange = ConnectionChangeBuilder.SerializeConnection(connection);
            connectionChange.ChangedProperties = null;
            connections.Add(connection.Id, connectionChange);
        }

        var change = new RoomChange
        {
            ChangedProperties = null,
            Actors = actors,
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

    private static void SerializeMap(RoomComponent room, RoomChange change)
    {
        var tileCount = room.TileCount;
        var knownTerrainChanges = new List<(short, MapFeature)>();
        var wallNeighborsChanges = new List<(short, DirectionFlags)>();
        for (short j = 0; j < tileCount; j++)
        {
            var feature = room.KnownTerrain[j];
            if (feature == MapFeature.Unexplored)
            {
                continue;
            }

            knownTerrainChanges.Add((j, feature));
            var neighbors = room.Neighbors[j];
            if (neighbors != DirectionFlags.None)
            {
                wallNeighborsChanges.Add((j, neighbors));
            }
        }

        var visibleTerrainChanges = new List<(short, byte)>();
        for (short j = 0; j < tileCount; j++)
        {
            var visibility = room.VisibleTerrain[j];
            if (visibility != 0)
            {
                visibleTerrainChanges.Add((j, visibility));
            }
        }

        if (knownTerrainChanges.Count == 0 && wallNeighborsChanges.Count == 0 && visibleTerrainChanges.Count == 0)
        {
            // No map data
        }
        else if ((knownTerrainChanges.Count + wallNeighborsChanges.Count + visibleTerrainChanges.Count) > tileCount)
        {
            change.LevelMap = new RoomMap
            {
                Terrain = room.KnownTerrain,
                WallNeighbors = TerrainChangeBuilder.MaskWallNeighborsToExplored(room),
                VisibleTerrain = room.VisibleTerrain
            };
        }
        else
        {
            if (knownTerrainChanges.Count == 0 && wallNeighborsChanges.Count == 0)
            {
                change.LevelMapChanges = new RoomMapChanges
                {
                    VisibleTerrainChanges = visibleTerrainChanges
                };
            }
            else
            {
                change.LevelMapChanges = new RoomMapChanges
                {
                    ChangedProperties = null,
                    TerrainChanges = knownTerrainChanges,
                    WallNeighborsChanges = wallNeighborsChanges,
                    VisibleTerrainChanges = visibleTerrainChanges
                };
            }
        }
    }

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
        _actorBuilder.ResetBaseline(playerEntity);
        foreach (var existing in PlayerBuilders)
        {
            if (existing.PlayerEntityId == playerEntity.Id)
            {
                existing.Reset(playerEntity);
                return;
            }
        }

        var builder = new PlayerChangeBuilder(playerEntity, this);
        builder.RegisterOnGroups(manager);
        PlayerBuilders.Add(builder);
    }

    internal void ResetActorBaseline(GameEntity playerEntity)
        => _actorBuilder.ResetBaseline(playerEntity);

    internal void TrackObserverTelegraphVisibility(GameEntity playerEntity)
        => _actorBuilder.TrackObserverTelegraphVisibility(playerEntity);

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
