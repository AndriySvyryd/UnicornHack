using System.Diagnostics;
using UnicornHack.Hubs.ChangeTracking;

namespace UnicornHack.TestUtilities;

public static class ClientStateHelpers
{
    public static PlayerChange Deserialize(PlayerChange data)
    {
        var player = new PlayerChange
        {
            ChangedProperties = null,
            Name = data.Name,
            Id = data.Id,
            PreviousTick = data.PreviousTick,
            CurrentTick = data.CurrentTick,
            NextActionTick = data.NextActionTick,
            XP = data.XP,
            Hp = data.Hp,
            MaxHp = data.MaxHp,
            Ep = data.Ep,
            MaxEp = data.MaxEp,
            ReservedEp = data.ReservedEp,
            Fortune = data.Fortune,
            Races = new Dictionary<int, RaceChange>(),
            Abilities = new Dictionary<int, AbilityChange>(),
            Log = new Dictionary<int, LogEntryChange>()
        };

        DeserializeLevel(data.Level!, player);

        foreach (var kvp in data.Races!)
        {
            ExpandRace(kvp.Key, kvp.Value, player.Races!, isParentAdd: true);
        }

        foreach (var kvp in data.Abilities!)
        {
            ExpandAbility(kvp.Key, kvp.Value, player.Abilities!, isParentAdd: true);
        }

        foreach (var kvp in data.Log!)
        {
            ExpandLogEntry(kvp.Key, kvp.Value, player.Log!, isParentAdd: true);
        }

        return player;
    }

    public static void ApplyChangeSets(PlayerChange player, List<TurnChangeSet> changeSets)
    {
        foreach (var changeSet in changeSets)
        {
            ApplyDelta(player, changeSet.PlayerState);
        }
    }

    public static void ApplyDelta(PlayerChange player, PlayerChange data)
    {
        var setValues = data.ChangedProperties!;

        if (setValues[3])
        {
            player.PreviousTick = data.PreviousTick;
        }

        if (setValues[4])
        {
            player.CurrentTick = data.CurrentTick;
        }

        if (setValues[5])
        {
            if (data.Level!.ChangedProperties != null)
            {
                ApplyLevelDelta(player, data.Level);
            }
            else
            {
                DeserializeLevel(data.Level, player);
            }
        }

        if (setValues[6])
        {
            foreach (var kvp in data.Races!)
            {
                ExpandRace(kvp.Key, kvp.Value, player.Races!, isParentAdd: false);
            }
        }

        if (setValues[7])
        {
            foreach (var kvp in data.Abilities!)
            {
                ExpandAbility(kvp.Key, kvp.Value, player.Abilities!, isParentAdd: false);
            }
        }

        if (setValues[8])
        {
            foreach (var kvp in data.Log!)
            {
                ExpandLogEntry(kvp.Key, kvp.Value, player.Log!, isParentAdd: false);
            }
        }

        if (setValues[9])
        {
            player.NextActionTick = data.NextActionTick;
        }

        if (setValues[10])
        {
            player.XP = data.XP;
        }

        if (setValues[11])
        {
            player.Hp = data.Hp;
        }

        if (setValues[12])
        {
            player.MaxHp = data.MaxHp;
        }

        if (setValues[13])
        {
            player.Ep = data.Ep;
        }

        if (setValues[14])
        {
            player.MaxEp = data.MaxEp;
        }

        if (setValues[15])
        {
            player.ReservedEp = data.ReservedEp;
        }

        if (setValues[16])
        {
            player.Fortune = data.Fortune;
        }
    }

    private static void DeserializeLevel(LevelChange data, PlayerChange player)
    {
        var levelMap = new LevelMap();
        if (data.LevelMap is { } fullMap)
        {
            levelMap.Terrain = (byte[])fullMap.Terrain.Clone();
            levelMap.WallNeighbors = (byte[])fullMap.WallNeighbors.Clone();
            levelMap.VisibleTerrain = (byte[])fullMap.VisibleTerrain.Clone();
        }
        else if (data.LevelMapChanges is { } sparseMap)
        {
            InitializeEmptyMap(levelMap, data.Width, data.Height);
            ApplySparseMapChanges(sparseMap, levelMap);
        }

        player.Level = new LevelChange
        {
            ChangedProperties = null,
            BranchName = data.BranchName,
            Depth = data.Depth,
            Width = data.Width,
            Height = data.Height,
            Actors = new Dictionary<int, ActorChange>(),
            Items = new Dictionary<int, ItemChange>(),
            Connections = new Dictionary<int, ConnectionChange>(),
            LevelMap = levelMap
        };
        var level = player.Level;

        if (data.Actors != null)
        {
            foreach (var kvp in data.Actors)
            {
                ExpandActor(kvp.Key, kvp.Value, level.Actors!, isParentAdd: true);
            }
        }

        if (data.Items != null)
        {
            foreach (var kvp in data.Items)
            {
                ExpandItem(kvp.Key, kvp.Value, level.Items!, isParentAdd: true);
            }
        }

        if (data.Connections != null)
        {
            foreach (var kvp in data.Connections)
            {
                ExpandConnection(kvp.Key, kvp.Value, level.Connections!, isParentAdd: true);
            }
        }
    }

    private static void InitializeEmptyMap(LevelMap levelMap, int width, int height)
    {
        var tileCount = width * height;
        levelMap.Terrain = new byte[tileCount];
        levelMap.WallNeighbors = new byte[tileCount];
        levelMap.VisibleTerrain = new byte[tileCount];
        Array.Fill(levelMap.Terrain, (byte)254);
    }

    private static void ApplyLevelDelta(PlayerChange player, LevelChange data)
    {
        var setValues = data.ChangedProperties!;
        var level = player.Level!;
        var levelMap = level.LevelMap!;

        for (var i = 1; i < setValues.Length; i++)
        {
            if (!setValues[i])
            {
                continue;
            }

            switch (i)
            {
                case 1:
                    if (data.Actors != null)
                    {
                        foreach (var kvp in data.Actors)
                        {
                            ExpandActor(kvp.Key, kvp.Value, level.Actors!, isParentAdd: false);
                        }
                    }

                    break;
                case 2:
                    if (data.Items != null)
                    {
                        foreach (var kvp in data.Items)
                        {
                            ExpandItem(kvp.Key, kvp.Value, level.Items!, isParentAdd: false);
                        }
                    }

                    break;
                case 3:
                    if (data.Connections != null)
                    {
                        foreach (var kvp in data.Connections)
                        {
                            ExpandConnection(kvp.Key, kvp.Value, level.Connections!, isParentAdd: false);
                        }
                    }

                    break;
                case 4:
                    var fullMap = data.LevelMap!;
                    var tileCount = level.Width * level.Height;
                    Array.Copy(fullMap.Terrain, levelMap.Terrain, tileCount);
                    Array.Copy(fullMap.WallNeighbors, levelMap.WallNeighbors, tileCount);
                    Array.Copy(fullMap.VisibleTerrain, levelMap.VisibleTerrain, tileCount);
                    break;
                case 5:
                    ApplySparseMapChanges(data.LevelMapChanges!, levelMap);
                    break;
            }
        }
    }

    private static void ApplySparseMapChanges(LevelMapChanges sparseMap, LevelMap levelMap)
    {
        var setValues = sparseMap.ChangedProperties;
        if (setValues != null)
        {
            if (setValues[1] && sparseMap.TerrainChanges != null)
            {
                ApplyTerrainChanges(sparseMap.TerrainChanges, levelMap.Terrain);
            }

            if (setValues[2] && sparseMap.WallNeighborsChanges != null)
            {
                ApplyTerrainChanges(sparseMap.WallNeighborsChanges, levelMap.WallNeighbors);
            }

            if (setValues[3] && sparseMap.VisibleTerrainChanges != null)
            {
                ApplyTerrainChanges(sparseMap.VisibleTerrainChanges, levelMap.VisibleTerrain);
            }
        }
        else
        {
            if (sparseMap.TerrainChanges != null)
            {
                ApplyTerrainChanges(sparseMap.TerrainChanges, levelMap.Terrain);
            }

            if (sparseMap.WallNeighborsChanges != null)
            {
                ApplyTerrainChanges(sparseMap.WallNeighborsChanges, levelMap.WallNeighbors);
            }

            if (sparseMap.VisibleTerrainChanges != null)
            {
                ApplyTerrainChanges(sparseMap.VisibleTerrainChanges, levelMap.VisibleTerrain);
            }
        }
    }

    private static void ApplyTerrainChanges(List<(short Index, byte Value)> changes, byte[] target)
    {
        foreach (var (index, value) in changes)
        {
            target[index] = value;
        }
    }

    public static void ExpandActor(
        int id, ActorChange data, Dictionary<int, ActorChange> collection, bool isParentAdd)
    {
        if (isParentAdd)
        {
            collection[id] = data;
            return;
        }

        var setValues = data.ChangedProperties;
        if (setValues == null)
        {
            Debug.Assert(!collection.ContainsKey(id),
                $"Actor {id} already exists but received ChangedProperties=null (expected delta with bits set)");
            collection[id] = data;
            return;
        }

        if (setValues[0])
        {
            if (!collection.TryGetValue(id, out var existing))
            {
                throw new InvalidOperationException(
                    $"Actor {id} not found: server emitted a Modified delta for an actor the client was never told about "
                    + "(missing prior Added emission).");
            }

            for (var i = 1; i < setValues.Length; i++)
            {
                if (!setValues[i])
                {
                    continue;
                }

                switch (i)
                {
                    case 1:
                        existing.LevelX = data.LevelX;
                        break;
                    case 2:
                        existing.LevelY = data.LevelY;
                        break;
                    case 3:
                        existing.Heading = data.Heading;
                        break;
                    case 4:
                        existing.BaseName = data.BaseName;
                        break;
                    case 5:
                        existing.Name = data.Name;
                        break;
                    case 6:
                        existing.IsCurrentlyPerceived = data.IsCurrentlyPerceived;
                        break;
                    case 7:
                        existing.Hp = data.Hp;
                        break;
                    case 8:
                        existing.MaxHp = data.MaxHp;
                        break;
                    case 9:
                        existing.Ep = data.Ep;
                        break;
                    case 10:
                        existing.MaxEp = data.MaxEp;
                        break;
                    case 11:
                        existing.NextActionTick = data.NextActionTick;
                        break;
                    case 12:
                        existing.NextAction = data.NextAction;
                        break;
                    case 13:
                        existing.MeleeAttack = data.MeleeAttack;
                        break;
                    case 14:
                        existing.RangeAttack = data.RangeAttack;
                        break;
                    case 15:
                        existing.MeleeDefense = data.MeleeDefense;
                        break;
                    case 16:
                        existing.RangeDefense = data.RangeDefense;
                        break;
                }
            }
        }
        else
        {
            if (!collection.Remove(id))
            {
                throw new InvalidOperationException(
                    $"Actor {id} not found: server emitted a Deleted sentinel for an actor the client was never told about "
                    + "(missing prior Added emission).");
            }
        }
    }

    public static void ExpandItem(
        int id, ItemChange data, Dictionary<int, ItemChange> collection, bool isParentAdd)
    {
        if (isParentAdd)
        {
            collection[id] = data;
            return;
        }

        var setValues = data.ChangedProperties;
        if (setValues == null)
        {
            Debug.Assert(!collection.ContainsKey(id),
                $"Item {id} already exists but received ChangedProperties=null (expected delta with bits set)");
            collection[id] = data;
            return;
        }

        if (setValues[0])
        {
            if (!collection.TryGetValue(id, out var existing))
            {
                throw new InvalidOperationException(
                    $"Item {id} not found: server emitted a Modified delta for an item the client was never told about "
                    + "(missing prior Added emission).");
            }

            for (var i = 1; i < setValues.Length; i++)
            {
                if (!setValues[i])
                {
                    continue;
                }

                switch (i)
                {
                    case 1:
                        existing.LevelX = data.LevelX;
                        break;
                    case 2:
                        existing.LevelY = data.LevelY;
                        break;
                    case 3:
                        existing.Type = data.Type;
                        break;
                    case 4:
                        existing.BaseName = data.BaseName;
                        break;
                    case 5:
                        existing.Name = data.Name;
                        break;
                    case 6:
                        existing.IsCurrentlyPerceived = data.IsCurrentlyPerceived;
                        break;
                }
            }
        }
        else
        {
            if (!collection.Remove(id))
            {
                throw new InvalidOperationException(
                    $"Item {id} not found: server emitted a Deleted sentinel for an item the client was never told about "
                    + "(missing prior Added emission).");
            }
        }
    }

    public static void ExpandConnection(
        int id, ConnectionChange data, Dictionary<int, ConnectionChange> collection, bool isParentAdd)
    {
        if (isParentAdd)
        {
            collection[id] = data;
            return;
        }

        var setValues = data.ChangedProperties;
        if (setValues == null)
        {
            Debug.Assert(!collection.ContainsKey(id),
                $"Connection {id} already exists but received ChangedProperties=null (expected delta with bits set)");
            collection[id] = data;
            return;
        }

        if (setValues[0])
        {
            if (!collection.TryGetValue(id, out var existing))
            {
                throw new InvalidOperationException(
                    $"Connection {id} not found: server emitted a Modified delta for a connection the client was never told about "
                    + "(missing prior Added emission).");
            }

            for (var i = 1; i < setValues.Length; i++)
            {
                if (!setValues[i])
                {
                    continue;
                }

                switch (i)
                {
                    case 1:
                        existing.LevelX = data.LevelX;
                        break;
                    case 2:
                        existing.LevelY = data.LevelY;
                        break;
                    case 3:
                        existing.IsDown = data.IsDown;
                        break;
                }
            }
        }
        else
        {
            if (!collection.Remove(id))
            {
                throw new InvalidOperationException(
                    $"Connection {id} not found: server emitted a Deleted sentinel for a connection the client was never told about "
                    + "(missing prior Added emission).");
            }
        }
    }

    public static void ExpandRace(
        int id, RaceChange data, Dictionary<int, RaceChange> collection, bool isParentAdd)
    {
        if (isParentAdd)
        {
            collection[id] = data;
            return;
        }

        var setValues = data.ChangedProperties;
        if (setValues == null)
        {
            Debug.Assert(!collection.ContainsKey(id),
                $"Race {id} already exists but received ChangedProperties=null (expected delta with bits set)");
            collection[id] = data;
            return;
        }

        if (setValues[0])
        {
            if (!collection.TryGetValue(id, out var existing))
            {
                throw new InvalidOperationException($"Race {id} not found for update");
            }

            for (var i = 1; i < setValues.Length; i++)
            {
                if (!setValues[i])
                {
                    continue;
                }

                switch (i)
                {
                    case 1:
                        existing.Name = data.Name;
                        break;
                    case 2:
                        existing.ShortName = data.ShortName;
                        break;
                }
            }
        }
        else if (!collection.Remove(id))
        {
            throw new InvalidOperationException($"Race {id} not found for removal");
        }
    }

    public static void ExpandAbility(
        int id, AbilityChange data, Dictionary<int, AbilityChange> collection, bool isParentAdd)
    {
        if (isParentAdd)
        {
            collection[id] = data;
            return;
        }

        var setValues = data.ChangedProperties;
        if (setValues == null)
        {
            Debug.Assert(!collection.ContainsKey(id),
                $"Ability {id} already exists but received ChangedProperties=null (expected delta with bits set)");
            collection[id] = data;
            return;
        }

        if (setValues[0])
        {
            if (!collection.TryGetValue(id, out var existing))
            {
                throw new InvalidOperationException(
                    $"Ability {id} not found: server emitted a Modified delta for an ability the client was never told about "
                    + "(missing prior Added emission).");
            }

            for (var i = 1; i < setValues.Length; i++)
            {
                if (!setValues[i])
                {
                    continue;
                }

                switch (i)
                {
                    case 1:
                        existing.Name = data.Name;
                        break;
                    case 2:
                        existing.Activation = data.Activation;
                        break;
                    case 3:
                        existing.Slot = data.Slot;
                        break;
                    case 4:
                        existing.CooldownTick = data.CooldownTick;
                        break;
                    case 5:
                        existing.CooldownXpLeft = data.CooldownXpLeft;
                        break;
                    case 6:
                        existing.TargetingShape = data.TargetingShape;
                        break;
                    case 7:
                        existing.TargetingShapeSize = data.TargetingShapeSize;
                        break;
                }
            }
        }
        else
        {
            if (!collection.Remove(id))
            {
                throw new InvalidOperationException(
                    $"Ability {id} not found: server emitted a Deleted sentinel for an ability the client was never told about "
                    + "(missing prior Added emission).");
            }
        }
    }

    public static void ExpandLogEntry(
        int id, LogEntryChange data, Dictionary<int, LogEntryChange> collection, bool isParentAdd)
    {
        if (isParentAdd)
        {
            collection[id] = data;
            return;
        }

        var setValues = data.ChangedProperties;
        if (setValues == null)
        {
            Debug.Assert(!collection.ContainsKey(id),
                $"LogEntry {id} already exists but received ChangedProperties=null (expected delta with bits set)");
            collection[id] = data;
            return;
        }

        if (setValues[0])
        {
            if (!collection.TryGetValue(id, out var existing))
            {
                throw new InvalidOperationException($"LogEntry {id} not found for update");
            }

            for (var i = 1; i < setValues.Length; i++)
            {
                if (!setValues[i])
                {
                    continue;
                }

                switch (i)
                {
                    case 1:
                        existing.Message = data.Message;
                        break;
                    case 2:
                        existing.Ticks = data.Ticks;
                        break;
                }
            }
        }
        else
        {
            if (!collection.Remove(id))
            {
                throw new InvalidOperationException($"LogEntry {id} not found for removal");
            }
        }
    }
}
