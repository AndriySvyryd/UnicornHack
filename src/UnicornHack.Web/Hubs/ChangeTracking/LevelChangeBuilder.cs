using UnicornHack.Services;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Hubs.ChangeTracking;

public class LevelChangeBuilder
{
    private readonly ActorChangeBuilder _actorBuilder = new();
    private readonly ItemChangeBuilder _itemBuilder = new();
    private readonly ConnectionChangeBuilder _connectionBuilder = new();

    public List<PlayerChangeBuilder> PlayerBuilders { get; } = [];

    public bool HasChanges(LevelComponent? level)
        => _actorBuilder.HasChanges || _itemBuilder.HasChanges || _connectionBuilder.HasChanges
           || (level != null && TerrainChangeBuilder.HasChanges(level));

    public LevelChange? GetSerializedLevel(
        GameEntity? levelEntity, GameEntity observer, GameServices services)
    {
        var level = levelEntity?.Level;
        var hasTerrainChanges = level != null && TerrainChangeBuilder.HasChanges(level);
        if (!_actorBuilder.HasChanges && !_itemBuilder.HasChanges
            && !_connectionBuilder.HasChanges && !hasTerrainChanges)
        {
            return null;
        }

        var context = new SerializationContext(null!, observer, services);
        var change = new LevelChange();

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
            TerrainChangeBuilder.WriteToLevelChange(level!, change);
        }

        return change;
    }

    public void Clear()
    {
        _actorBuilder.Clear();
        _itemBuilder.Clear();
        _connectionBuilder.Clear();

        foreach (var builder in PlayerBuilders)
        {
            builder.Clear();
        }
    }

    public static LevelChange SerializeLevel(
        GameEntity levelEntity, SerializationContext context)
    {
        var level = levelEntity.Level!;

        var actors = new Dictionary<int, ActorChange>();
        foreach (var actor in level.KnownActors.Values
                     .Select(a => a.Knowledge!).Select(a => a.Entity))
        {
            actors.Add(actor.Id, ActorChangeBuilder.SerializeActor(actor, context));
        }

        var items = new Dictionary<int, ItemChange>();
        foreach (var item in level.KnownItems.Values
                     .Select(t => t.Knowledge!).Select(t => t.Entity))
        {
            items.Add(item.Id, ItemChangeBuilder.SerializeItem(item, context));
        }

        var connections = new Dictionary<int, ConnectionChange>();
        foreach (var connection in level.KnownConnections.Values
                     .Select(c => c.Knowledge!)
                     .Where(c => c.KnownEntity.Connection!.Direction == null
                                 || (c.KnownEntity.Connection.Direction & ConnectionDirection.Source) != 0)
                     .Select(c => c.Entity))
        {
            var connectionChange = ConnectionChangeBuilder.SerializeConnection(connection);
            connectionChange.ChangedProperties = null;
            connections.Add(connection.Id, connectionChange);
        }

        var change = new LevelChange
        {
            ChangedProperties = null,
            Actors = actors,
            Items = items,
            Connections = connections,
            BranchName = level.BranchName,
            Depth = level.Depth,
            Width = level.Width,
            Height = level.Height
        };

        SerializeMap(level, change);
        return change;
    }

    private static void SerializeMap(LevelComponent level, LevelChange change)
    {
        var tileCount = level.TileCount;
        var knownTerrainChanges = new List<(short, byte)>();
        var wallNeighborsChanges = new List<(short, byte)>();
        for (short j = 0; j < tileCount; j++)
        {
            var feature = level.KnownTerrain[j];
            if (feature == (byte)MapFeature.Unexplored)
            {
                continue;
            }

            knownTerrainChanges.Add((j, feature));
            var neighbors = level.WallNeighbors[j];
            if (neighbors != 0)
            {
                wallNeighborsChanges.Add((j, neighbors));
            }
        }

        var visibleTerrainChanges = new List<(short, byte)>();
        for (short j = 0; j < tileCount; j++)
        {
            var visibility = level.VisibleTerrain[j];
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
            change.LevelMap = new LevelMap
            {
                Terrain = level.KnownTerrain,
                WallNeighbors = TerrainChangeBuilder.MaskWallNeighborsToExplored(level),
                VisibleTerrain = level.VisibleTerrain
            };
        }
        else
        {
            if (knownTerrainChanges.Count == 0 && wallNeighborsChanges.Count == 0)
            {
                change.LevelMapChanges = new LevelMapChanges
                {
                    VisibleTerrainChanges = visibleTerrainChanges
                };
            }
            else
            {
                change.LevelMapChanges = new LevelMapChanges
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
