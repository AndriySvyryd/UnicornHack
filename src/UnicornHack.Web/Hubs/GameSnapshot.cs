using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Data;
using UnicornHack.Services;

namespace UnicornHack.Hubs;

public class GameSnapshot
{
    public int SnapshotTick
    {
        get;
        set;
    } = -1;

    public Dictionary<int, PlayerSnapshot> PlayerSnapshots
    {
        get;
    } = new();

    public Dictionary<int, LevelSnapshot> LevelSnapshots
    {
        get;
    } = new();

    private Dictionary<int, List<object>> SerializedLevels
    {
        get;
    } = new();

    // TODO: Perf: Use change notifications instead
    public void CaptureState(GameDbContext dbContext, GameServices services)
    {
        SerializedLevels.Clear();

        var manager = dbContext.Manager;

        if (SnapshotTick == manager.Game.CurrentTick)
        {
            return;
        }

        SnapshotTick = manager.Game.CurrentTick;
        var snapshotedLevels = new List<int>();
        foreach (var playerEntity in manager.Players)
        {
            var context = new SerializationContext(dbContext, playerEntity, services);
            var levelEntity = playerEntity.Position.LevelEntity;
            if (!snapshotedLevels.Contains(levelEntity.Id))
            {
                if (!LevelSnapshots.TryGetValue(levelEntity.Id, out var levelSnapshot))
                {
                    levelSnapshot = new LevelSnapshot();
                    LevelSnapshots[levelEntity.Id] = levelSnapshot;
                }

                levelSnapshot.CaptureState(levelEntity, context);
                snapshotedLevels.Add(levelEntity.Id);
            }

            if (!PlayerSnapshots.TryGetValue(playerEntity.Id, out var playerSnapshot))
            {
                playerSnapshot = new PlayerSnapshot();
                PlayerSnapshots[playerEntity.Id] = playerSnapshot;
            }

            playerSnapshot.CaptureState(playerEntity, context);
        }

        if (snapshotedLevels.Count != LevelSnapshots.Count)
        {
            foreach (var levelId in LevelSnapshots.Keys.ToList())
            {
                if (!snapshotedLevels.Contains(levelId))
                {
                    LevelSnapshots.Remove(levelId);
                }
            }
        }

        if (manager.Players.Count != PlayerSnapshots.Count)
        {
            foreach (var playerId in PlayerSnapshots.Keys.ToList())
            {
                if (!manager.Players.ContainsEntity(playerId))
                {
                    PlayerSnapshots.Remove(playerId);
                }
            }
        }
    }

    public static List<object> Serialize(
        GameEntity playerEntity, EntityState state, GameSnapshot snapshot, SerializationContext context)
    {
        var levelEntity = playerEntity.Position.LevelEntity;
        LevelSnapshot levelSnapshot = null;
        if (snapshot != null
            && !snapshot.LevelSnapshots.TryGetValue(levelEntity.Id, out levelSnapshot))
        {
            levelSnapshot = new LevelSnapshot();
            snapshot.LevelSnapshots[levelEntity.Id] = levelSnapshot;
        }

        if (state == EntityState.Added)
        {
            PlayerSnapshot playerSnapshot = null;
            if (snapshot != null)
            {
                snapshot.SnapshotTick = context.Manager.Game.CurrentTick;
                playerSnapshot = new PlayerSnapshot();
                snapshot.PlayerSnapshots[playerEntity.Id] = playerSnapshot;
            }

            var serializedLevel = LevelSnapshot.Serialize(levelEntity, null, levelSnapshot, context);
            return PlayerSnapshot.Serialize(playerEntity, state, playerSnapshot, serializedLevel, context);
        }
        else
        {
            snapshot.SnapshotTick = context.Manager.Game.CurrentTick;
            if (!snapshot.SerializedLevels.TryGetValue(levelEntity.Id, out var serializedLevel))
            {
                var position = playerEntity.Position;
                var level = context.DbContext.Entry(position).Property(p => p.LevelId);
                serializedLevel = LevelSnapshot.Serialize
                (position.LevelEntity, level.IsModified
                        ? EntityState.Added
                        : state,
                    levelSnapshot, context);
                snapshot.SerializedLevels[levelEntity.Id] = serializedLevel;
            }

            return PlayerSnapshot.Serialize(
                playerEntity, state, snapshot.PlayerSnapshots[playerEntity.Id], serializedLevel, context);
        }
    }
}
