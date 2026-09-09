using UnicornHack.Data;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Services;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Time;

namespace UnicornHack.Hubs;

public static class GameStateManager
{
    /// <summary>
    ///     Pure read of the current player/level state. Safe to call from any thread and
    ///     does not mutate any change-tracking baseline: multiple clients (or repeat calls)
    ///     must observe identical state.
    /// </summary>
    public static PlayerChange GetState(GameEntity playerEntity, GameServices services)
    {
        var context = new SerializationContext(null, playerEntity, services);
        return PlayerChangeBuilder.SerializePlayer(playerEntity, in context);
    }

    /// <summary>
    ///     Advances the game until the current player is ready to act again, optionally
    ///     accumulating <see cref="TurnChangeSet" />s per affected player.
    ///     <para>
    ///         When <paramref name="dbContext" /> is provided, terrain property dirtiness
    ///         is marked on the EF change tracker between turns so the eventual
    ///         <c>SaveChanges</c> persists the room state. Tests/perf scenarios that don't
    ///         persist anything pass <c>null</c>.
    ///     </para>
    /// </summary>
    public static Dictionary<int, List<TurnChangeSet>> Turn(
        PlayerComponent currentPlayer,
        RoomChangeBuilder roomChangeBuilder,
        GameServices services,
        bool collectChanges = true,
        GameDbContext? dbContext = null)
    {
        var manager = currentPlayer.Game.Manager;
        var builders = collectChanges ? roomChangeBuilder.PlayerBuilders : null;
        var changeSetsAccumulator = new Dictionary<int, List<TurnChangeSet>>();

        currentPlayer.Game.ActingPlayer = null;

        if (collectChanges)
        {
            foreach (var builder in builders!)
            {
                builder.LastSentTick = manager.Game.CurrentTick;
            }
        }

        while (true)
        {
            if (collectChanges)
            {
                ClearTerrainChangeDicts(manager);
                CaptureVisibleTerrainSnapshots(manager);
            }

            var result = manager.TimeSystem.AdvanceSingleTurn(manager);

            if (collectChanges)
            {
                ComputeVisibleTerrainChanges(manager);
                if (dbContext is not null)
                {
                    MarkTerrainPropertiesModified(dbContext, manager);
                }

                roomChangeBuilder.PrepareCommonRoom(
                    currentPlayer.Entity.Position?.RoomEntity, services);

                foreach (var builder in builders!)
                {
                    if (builder.HasObservableChanges(manager)
                        || result is TurnResult.PlayerTurn or TurnResult.GameOver)
                    {
                        var changeSet = new TurnChangeSet
                        {
                            PlayerState = builder.BuildChangeSet(manager, services),
                            Events = roomChangeBuilder.EventBuilder.GetEvents(builder.PlayerEntityId)
                        };

                        if (!changeSetsAccumulator.TryGetValue(builder.PlayerEntityId, out var list))
                        {
                            list = [];
                            changeSetsAccumulator[builder.PlayerEntityId] = list;
                        }

                        list.Add(changeSet);
                    }
                }
            }

            roomChangeBuilder.Reset(manager);

            if (result is TurnResult.PlayerTurn or TurnResult.GameOver)
            {
                break;
            }
        }

        return changeSetsAccumulator;
    }

    public static void ClearTerrainChangeDicts(GameManager manager)
    {
        foreach (var roomEntity in manager.Rooms)
        {
            var room = roomEntity.Room!;
            if (room.TerrainChanges != null)
            {
                room.TerrainChanges.Clear();
            }
            else
            {
                room.TerrainChanges = [];
            }

            if (room.KnownTerrainChanges != null)
            {
                room.KnownTerrainChanges.Clear();
            }
            else
            {
                room.KnownTerrainChanges = [];
            }

            if (room.NeighborChanges != null)
            {
                room.NeighborChanges.Clear();
            }
            else
            {
                room.NeighborChanges = [];
            }

            room.KnownNeighborsChanged = false;
        }
    }

    public static void CaptureVisibleTerrainSnapshots(GameManager manager)
    {
        foreach (var roomEntity in manager.Rooms)
        {
            var room = roomEntity.Room!;
            if (room.Width == 0)
            {
                continue;
            }

            if (room.VisibleTerrainSnapshot == null)
            {
                room.VisibleTerrainSnapshot = (byte[])room.VisibleTerrain.Clone();
            }
            else
            {
                room.VisibleTerrain.CopyTo(room.VisibleTerrainSnapshot, 0);
            }
        }
    }

    public static void ComputeVisibleTerrainChanges(GameManager manager)
    {
        foreach (var roomEntity in manager.Rooms)
        {
            var room = roomEntity.Room!;
            if (room.Width == 0)
            {
                continue;
            }

            if (room.VisibleTerrainChanges == null)
            {
                room.VisibleTerrainChanges = [];
            }
            else
            {
                room.VisibleTerrainChanges.Clear();
            }

            if (room.VisibleTerrainSnapshot != null)
            {
                var tileCount = room.TileCount;
                for (short i = 0; i < tileCount; i++)
                {
                    var newValue = room.VisibleTerrain[i];
                    if (newValue != room.VisibleTerrainSnapshot[i])
                    {
                        room.VisibleTerrainChanges.Add(i, newValue);
                    }
                }
            }
        }
    }

    private static void MarkTerrainPropertiesModified(GameDbContext dbContext, GameManager manager)
    {
        dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
        foreach (var roomEntity in manager.Rooms)
        {
            var room = roomEntity.Room!;

            var levelEntry = dbContext.Entry(room);
            if (levelEntry.State == EntityState.Added
                || room.Width == 0)
            {
                continue;
            }

            if (room.TerrainChanges == null
                || room.TerrainChanges.Count > 0)
            {
                levelEntry.Property(l => l.Terrain).IsModified = true;
            }

            if (room.KnownTerrainChanges == null
                || room.KnownTerrainChanges.Count > 0)
            {
                levelEntry.Property(l => l.KnownTerrain).IsModified = true;
            }

            if (room.NeighborChanges == null
                || room.NeighborChanges.Count > 0)
            {
                levelEntry.Property(l => l.Neighbors).IsModified = true;
            }

            if (room.VisibleTerrainSnapshot == null
                || (room.VisibleTerrainChanges != null && room.VisibleTerrainChanges.Count > 0))
            {
                levelEntry.Property(l => l.VisibleTerrain).IsModified = true;
            }

            if (room.KnownNeighborsChanged)
            {
                levelEntry.Property(l => l.KnownNeighbors).IsModified = true;
            }
        }

        dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
    }
}
