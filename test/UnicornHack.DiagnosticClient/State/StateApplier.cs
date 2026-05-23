using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Primitives;

namespace UnicornHack.DiagnosticClient.State;

/// <summary>
///     Applies <see cref="PlayerChange" /> messages onto a canonical client-side
///     instance. Ports the Unity client's <c>PlayerChange.Expand</c>/<c>Update</c>
///     switch logic; see
///     <c>UnicornHackClient/Assets/Scripts/Transport/State/PlayerChange.cs</c>.
///     The DTOs are the server's own types (shared via ProjectReference). The
///     custom formatters in <c>UnicornHack.Web/Hubs/MessagePack/*</c> only invoke a
///     property setter when its <c>ChangedProperties</c> bit is true, so the setter
///     side effect (re-setting the same bit) is always a no-op on the deserialized
///     delta. On the canonical instance we keep <c>ChangedProperties</c> at <c>null</c>
///     so writes against it are also no-ops via the <c>?.</c> guard in each setter.
/// </summary>
internal static class StateApplier
{
    /// <summary>
    ///     Adopts a server full snapshot (<c>ChangedProperties == null</c>) as the canonical
    ///     state. Replaces the BitArray with a fully-set one so downstream callers can treat
    ///     all fields as "current".
    /// </summary>
    public static PlayerChange AdoptFullSnapshot(PlayerChange snapshot)
    {
        // Ensure non-null collections so reads don't need null guards everywhere.
        snapshot.Races ??= new Dictionary<int, RaceChange>();
        snapshot.Abilities ??= new Dictionary<int, AbilityChange>();
        snapshot.Log ??= new Dictionary<int, LogEntryChange>();

        if (snapshot.Room is { } room)
        {
            NormalizeRoom(room);
            // The initial GetState snapshot sends terrain as LevelMapChanges (sparse) when
            // the explored cell count fits inside the tile count. Materialize a LevelMap so
            // the renderer and other consumers can read room.LevelMap directly.
            if (room.LevelMap is null && room.LevelMapChanges is not null)
            {
                ApplyRoomMapChanges(room, room.LevelMapChanges);
                room.LevelMapChanges = null;
            }
        }

        return snapshot;
    }

    public static void ApplyDelta(PlayerChange canonical, PlayerChange delta)
    {
        var bits = delta.ChangedProperties;
        if (bits is null)
        {
            // Server treats null bits as a full snapshot; rare in deltas.
            ReplaceFromFull(canonical, delta);
            return;
        }

        canonical.PreviousTick = delta.PreviousTick;
        canonical.CurrentTick = delta.CurrentTick;

        for (var i = 5; i < bits.Length; i++)
        {
            if (!bits[i])
            {
                continue;
            }
            switch (i)
            {
                case 5:
                    canonical.Room = MergeRoom(canonical.Room, delta.Room);
                    break;
                case 6:
                    MergeRaces(canonical, delta.Races);
                    break;
                case 7:
                    MergeAbilities(canonical, delta.Abilities);
                    break;
                case 8:
                    MergeLog(canonical, delta.Log);
                    break;
                case 9: canonical.NextActionTick = delta.NextActionTick; break;
                case 10: canonical.XP = delta.XP; break;
                case 11: canonical.Hp = delta.Hp; break;
                case 12: canonical.MaxHp = delta.MaxHp; break;
                case 13: canonical.Ep = delta.Ep; break;
                case 14: canonical.MaxEp = delta.MaxEp; break;
                case 15: canonical.ReservedEp = delta.ReservedEp; break;
                case 16: canonical.Fortune = delta.Fortune; break;
            }
        }
    }

    private static void ReplaceFromFull(PlayerChange canonical, PlayerChange snapshot)
    {
        canonical.Name = snapshot.Name;
        canonical.Id = snapshot.Id;
        canonical.PreviousTick = snapshot.PreviousTick;
        canonical.CurrentTick = snapshot.CurrentTick;
        canonical.Room = snapshot.Room;
        canonical.Races = snapshot.Races ?? new Dictionary<int, RaceChange>();
        canonical.Abilities = snapshot.Abilities ?? new Dictionary<int, AbilityChange>();
        canonical.Log = snapshot.Log ?? new Dictionary<int, LogEntryChange>();
        canonical.NextActionTick = snapshot.NextActionTick;
        canonical.XP = snapshot.XP;
        canonical.Hp = snapshot.Hp;
        canonical.MaxHp = snapshot.MaxHp;
        canonical.Ep = snapshot.Ep;
        canonical.MaxEp = snapshot.MaxEp;
        canonical.ReservedEp = snapshot.ReservedEp;
        canonical.Fortune = snapshot.Fortune;
        if (canonical.Room is { } r)
        {
            NormalizeRoom(r);
        }
    }

    // ---------- Room ----------

    private static RoomChange MergeRoom(RoomChange? canonical, RoomChange? delta)
    {
        if (delta is null)
        {
            return canonical ?? new RoomChange();
        }

        var bits = delta.ChangedProperties;
        if (bits is null || canonical is null)
        {
            // Full room snapshot — adopt it.
            NormalizeRoom(delta);
            return delta;
        }

        for (var i = 1; i < bits.Length; i++)
        {
            if (!bits[i])
            {
                continue;
            }
            switch (i)
            {
                case 1: MergeActors(canonical, delta.Actors); break;
                case 2: MergeItems(canonical, delta.Items); break;
                case 3: MergeConnections(canonical, delta.Connections); break;
                case 4:
                    if (delta.LevelMap is not null)
                    {
                        canonical.LevelMap = delta.LevelMap;
                    }
                    break;
                case 5:
                    ApplyRoomMapChanges(canonical, delta.LevelMapChanges);
                    break;
                case 6: canonical.BranchName = delta.BranchName; break;
                case 7: canonical.Depth = delta.Depth; break;
                case 8: canonical.Width = delta.Width; break;
                case 9: canonical.Height = delta.Height; break;
                case 10: canonical.GridX = delta.GridX; break;
                case 11: canonical.GridY = delta.GridY; break;
            }
        }
        return canonical;
    }

    private static void NormalizeRoom(RoomChange room)
    {
        room.Actors ??= new Dictionary<int, ActorChange>();
        room.Items ??= new Dictionary<int, ItemChange>();
        room.Connections ??= new Dictionary<int, ConnectionChange>();
    }

    private static void ApplyRoomMapChanges(RoomChange room, RoomMapChanges? changes)
    {
        if (changes is null)
        {
            return;
        }

        // The server emits LevelMapChanges (instead of a full LevelMap) whenever the total
        // count of changes fits inside the tile count — including the initial state when the
        // client has no prior LevelMap. Lazily initialize one so those changes can be applied.
        if (room.LevelMap is not { } map)
        {
            var tileCount = room.Width * room.Height;
            map = new RoomMap
            {
                Terrain = new MapFeature[tileCount],
                WallNeighbors = new DirectionFlags[tileCount],
                VisibleTerrain = new byte[tileCount]
            };
            Array.Fill(map.Terrain, MapFeature.Unexplored);
            room.LevelMap = map;
        }

        if (changes.TerrainChanges is { } tc)
        {
            foreach (var (idx, value) in tc)
            {
                if (idx >= 0 && idx < map.Terrain.Length)
                {
                    map.Terrain[idx] = value;
                }
            }
        }
        if (changes.WallNeighborsChanges is { } wnc)
        {
            foreach (var (idx, value) in wnc)
            {
                if (idx >= 0 && idx < map.WallNeighbors.Length)
                {
                    map.WallNeighbors[idx] = value;
                }
            }
        }
        if (changes.VisibleTerrainChanges is { } vtc)
        {
            foreach (var (idx, value) in vtc)
            {
                if (idx >= 0 && idx < map.VisibleTerrain.Length)
                {
                    map.VisibleTerrain[idx] = value;
                }
            }
        }
    }

    private static void MergeActors(RoomChange room, Dictionary<int, ActorChange>? delta)
    {
        if (delta is null)
        {
            return;
        }
        room.Actors ??= new Dictionary<int, ActorChange>();
        foreach (var (id, child) in delta)
        {
            MergeChildActor(room.Actors, id, child);
        }
    }

    private static void MergeChildActor(Dictionary<int, ActorChange> store, int id, ActorChange delta)
    {
        var bits = delta.ChangedProperties;
        if (bits is null)
        {
            // Server full-snapshot for an Added entity (ProduceFullSnapshot returns null bits).
            store[id] = delta;
            return;
        }
        if (bits.Length <= 1)
        {
            // Removal sentinel: server emits `new BitArray(1)` for Deleted entries.
            store.Remove(id);
            return;
        }
        if (!store.TryGetValue(id, out var current))
        {
            // Delta arrived without prior snapshot (rare re-emission case): adopt as-is.
            store[id] = delta;
            return;
        }
        for (var i = 1; i < bits.Length; i++)
        {
            if (!bits[i]) continue;
            switch (i)
            {
                case 1: current.LevelX = delta.LevelX; break;
                case 2: current.LevelY = delta.LevelY; break;
                case 3: current.Heading = delta.Heading; break;
                case 4: current.BaseName = delta.BaseName; break;
                case 5: current.Name = delta.Name; break;
                case 6: current.IsCurrentlyPerceived = delta.IsCurrentlyPerceived; break;
                case 7: current.Hp = delta.Hp; break;
                case 8: current.MaxHp = delta.MaxHp; break;
                case 9: current.Ep = delta.Ep; break;
                case 10: current.MaxEp = delta.MaxEp; break;
                case 11: current.NextActionTick = delta.NextActionTick; break;
                case 12: current.NextAction = delta.NextAction; break;
                case 13: current.MeleeAttack = delta.MeleeAttack; break;
                case 14: current.RangeAttack = delta.RangeAttack; break;
                case 15: current.MeleeDefense = delta.MeleeDefense; break;
                case 16: current.RangeDefense = delta.RangeDefense; break;
            }
        }
    }

    private static void MergeItems(RoomChange room, Dictionary<int, ItemChange>? delta)
    {
        if (delta is null) return;
        room.Items ??= new Dictionary<int, ItemChange>();
        foreach (var (id, child) in delta)
        {
            MergeChildItem(room.Items, id, child);
        }
    }

    private static void MergeChildItem(Dictionary<int, ItemChange> store, int id, ItemChange delta)
    {
        var bits = delta.ChangedProperties;
        if (bits is null) { store[id] = delta; return; }
        if (bits.Length <= 1) { store.Remove(id); return; }
        if (!store.TryGetValue(id, out var current))
        {
            store[id] = delta;
            return;
        }
        for (var i = 1; i < bits.Length; i++)
        {
            if (!bits[i]) continue;
            switch (i)
            {
                case 1: current.LevelX = delta.LevelX; break;
                case 2: current.LevelY = delta.LevelY; break;
                case 3: current.Type = delta.Type; break;
                case 4: current.BaseName = delta.BaseName; break;
                case 5: current.Name = delta.Name; break;
                case 6: current.IsCurrentlyPerceived = delta.IsCurrentlyPerceived; break;
            }
        }
    }

    private static void MergeConnections(RoomChange room, Dictionary<int, ConnectionChange>? delta)
    {
        if (delta is null) return;
        room.Connections ??= new Dictionary<int, ConnectionChange>();
        foreach (var (id, child) in delta)
        {
            var bits = child.ChangedProperties;
            if (bits is null) { room.Connections[id] = child; continue; }
            if (bits.Length <= 1) { room.Connections.Remove(id); continue; }
            if (!room.Connections.TryGetValue(id, out var current))
            {
                room.Connections[id] = child;
                continue;
            }
            for (var i = 1; i < bits.Length; i++)
            {
                if (!bits[i]) continue;
                switch (i)
                {
                    case 1: current.LevelX = child.LevelX; break;
                    case 2: current.LevelY = child.LevelY; break;
                    case 3: current.Orientation = child.Orientation; break;
                }
            }
        }
    }

    private static void MergeRaces(PlayerChange player, Dictionary<int, RaceChange>? delta)
    {
        if (delta is null) return;
        player.Races ??= new Dictionary<int, RaceChange>();
        foreach (var (id, child) in delta)
        {
            var bits = child.ChangedProperties;
            if (bits is null) { player.Races[id] = child; continue; }
            if (bits.Length <= 1) { player.Races.Remove(id); continue; }
            if (!player.Races.TryGetValue(id, out var current))
            {
                player.Races[id] = child;
                continue;
            }
            for (var i = 1; i < bits.Length; i++)
            {
                if (!bits[i]) continue;
                switch (i)
                {
                    case 1: current.Name = child.Name; break;
                    case 2: current.ShortName = child.ShortName; break;
                }
            }
        }
    }

    private static void MergeAbilities(PlayerChange player, Dictionary<int, AbilityChange>? delta)
    {
        if (delta is null) return;
        player.Abilities ??= new Dictionary<int, AbilityChange>();
        foreach (var (id, child) in delta)
        {
            var bits = child.ChangedProperties;
            if (bits is null) { player.Abilities[id] = child; continue; }
            if (bits.Length <= 1) { player.Abilities.Remove(id); continue; }
            if (!player.Abilities.TryGetValue(id, out var current))
            {
                player.Abilities[id] = child;
                continue;
            }
            for (var i = 1; i < bits.Length; i++)
            {
                if (!bits[i]) continue;
                switch (i)
                {
                    case 1: current.Name = child.Name; break;
                    case 2: current.Activation = child.Activation; break;
                    case 3: current.Slot = child.Slot; break;
                    case 4: current.CooldownTick = child.CooldownTick; break;
                    case 5: current.CooldownXpLeft = child.CooldownXpLeft; break;
                    case 6: current.TargetingShape = child.TargetingShape; break;
                    case 7: current.TargetingShapeSize = child.TargetingShapeSize; break;
                }
            }
        }
    }

    private static void MergeLog(PlayerChange player, Dictionary<int, LogEntryChange>? delta)
    {
        if (delta is null) return;
        player.Log ??= new Dictionary<int, LogEntryChange>();
        foreach (var (id, child) in delta)
        {
            // Log entries are append-only; bits != null always carries full content.
            player.Log[id] = child;
        }
    }
}
