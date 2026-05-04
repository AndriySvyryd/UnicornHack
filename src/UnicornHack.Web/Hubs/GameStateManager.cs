using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Services;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Time;

namespace UnicornHack.Hubs;

public static class GameStateManager
{
    /// <summary>
    ///     Pure read of the current player/level state. Safe to call from any thread.
    /// </summary>
    public static PlayerChange GetState(GameEntity playerEntity, GameServices services)
        => PlayerChangeBuilder.SerializePlayer(
            playerEntity, new SerializationContext(null!, playerEntity, services));

    public static Dictionary<int, List<TurnChangeSet>> Turn(
        PlayerComponent currentPlayer,
        LevelChangeBuilder levelChangeBuilder,
        GameServices services,
        bool collectChanges = true,
        Action<GameManager>? onAfterEachTurn = null)
    {
        var manager = currentPlayer.Game.Manager;
        var builders = collectChanges ? levelChangeBuilder.PlayerBuilders : null;
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
                onAfterEachTurn?.Invoke(manager);

                foreach (var builder in builders!)
                {
                    if (builder.HasObservableChanges(manager)
                        || (result is TurnResult.PlayerTurn
                            && changeSetsAccumulator.ContainsKey(builder.PlayerEntityId)))
                    {
                        var changeSet = new TurnChangeSet
                        {
                            PlayerState = builder.BuildChangeSet(manager, services),
                            Events = null // TODO: Serialize messages
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

            levelChangeBuilder.Clear();

            if (result is TurnResult.PlayerTurn or TurnResult.GameOver)
            {
                break;
            }
        }

        return changeSetsAccumulator;
    }

    public static void ClearTerrainChangeDicts(GameManager manager)
    {
        foreach (var levelEntity in manager.Levels)
        {
            var level = levelEntity.Level!;
            if (level.TerrainChanges != null)
            {
                level.TerrainChanges.Clear();
            }
            else
            {
                level.TerrainChanges = [];
            }

            if (level.KnownTerrainChanges != null)
            {
                level.KnownTerrainChanges.Clear();
            }
            else
            {
                level.KnownTerrainChanges = [];
            }

            if (level.WallNeighborsChanges != null)
            {
                level.WallNeighborsChanges.Clear();
            }
            else
            {
                level.WallNeighborsChanges = [];
            }

            level.VisibleNeighborsChanged = false;
        }
    }

    public static void CaptureVisibleTerrainSnapshots(GameManager manager)
    {
        foreach (var levelEntity in manager.Levels)
        {
            var level = levelEntity.Level!;
            if (level.Width == 0)
            {
                continue;
            }

            if (level.VisibleTerrainSnapshot == null)
            {
                level.VisibleTerrainSnapshot = (byte[])level.VisibleTerrain.Clone();
            }
            else
            {
                level.VisibleTerrain.CopyTo(level.VisibleTerrainSnapshot, 0);
            }
        }
    }

    public static void ComputeVisibleTerrainChanges(GameManager manager)
    {
        foreach (var levelEntity in manager.Levels)
        {
            var level = levelEntity.Level!;
            if (level.Width == 0)
            {
                continue;
            }

            if (level.VisibleTerrainChanges == null)
            {
                level.VisibleTerrainChanges = [];
            }
            else
            {
                level.VisibleTerrainChanges.Clear();
            }

            if (level.VisibleTerrainSnapshot != null)
            {
                var tileCount = level.TileCount;
                for (short i = 0; i < tileCount; i++)
                {
                    var newValue = level.VisibleTerrain[i];
                    if (newValue != level.VisibleTerrainSnapshot[i])
                    {
                        level.VisibleTerrainChanges.Add(i, newValue);
                    }
                }
            }
        }
    }
}
