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
        => _actorBuilder.HasChanges || _itemBuilder.HasChanges || _connectionBuilder.HasChanges
           || (room != null && TerrainChangeBuilder.HasChanges(room));

    public RoomChange? GetSerializedRoom(
        GameEntity? roomEntity, GameEntity observer, GameServices services)
    {
        var room = roomEntity?.Room;
        var hasTerrainChanges = room != null && TerrainChangeBuilder.HasChanges(room);
        if (!_actorBuilder.HasChanges && !_itemBuilder.HasChanges
            && !_connectionBuilder.HasChanges && !hasTerrainChanges)
        {
            return null;
        }

        var context = new SerializationContext(null!, observer, services);
        var change = new RoomChange();

        if (_actorBuilder.HasChanges)
        {
            var serializedActors = new Dictionary<int, ActorChange>();
            _actorBuilder.WriteTo(serializedActors, context);
            change.Actors = serializedActors;
        }

        if (_itemBuilder.HasChanges)
        {
            var serializedItems = new Dictionary<int, ItemChange>();
            _itemBuilder.WriteTo(serializedItems, context);
            change.Items = serializedItems;
        }

        if (_connectionBuilder.HasChanges)
        {
            var serializedConnections = new Dictionary<int, ConnectionChange>();
            _connectionBuilder.WriteTo(serializedConnections, context);
            change.Connections = serializedConnections;
        }

        if (hasTerrainChanges)
        {
            TerrainChangeBuilder.WriteToRoomChange(room!, change);
        }

        return change;
    }

    public void Clear()
    {
        _actorBuilder.Clear();
        _itemBuilder.Clear();
        _connectionBuilder.Clear();
        _eventBuilder.Clear();

        foreach (var builder in PlayerBuilders)
        {
            builder.Clear();
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

    public void RegisterPlayerBuilder(GameEntity playerEntity, GameManager manager)
    {
        if (PlayerBuilders.Any(existing => existing.PlayerEntityId == playerEntity.Id))
        {
            return;
        }

        var builder = new PlayerChangeBuilder(playerEntity, this);
        builder.RegisterOnGroups(manager);
        PlayerBuilders.Add(builder);
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
