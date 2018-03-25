using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Data;
using UnicornHack.Services;

namespace UnicornHack.Hubs
{
    public class GameSnapshot
    {
        public Dictionary<int, PlayerSnapshot> PlayerSnapshots { get; set; } = new Dictionary<int, PlayerSnapshot>();
        public Dictionary<int, LevelSnapshot> LevelSnapshots { get; } = new Dictionary<int, LevelSnapshot>();
        private Dictionary<int, List<object>> SerializedLevels { get; } = new Dictionary<int, List<object>>();

        // TODO: Perf: Use change notifications instead
        public void Snapshot(GameDbContext dbContext, GameServices services)
        {
            SerializedLevels.Clear();
            var snapshotedLevels = new List<int>();

            var manager = dbContext.Manager;
            foreach (var playerEntity in manager.Players)
            {
                var context = new SerializationContext(dbContext, playerEntity, services);
                var levelEntity = playerEntity.Position.LevelEntity;
                LevelSnapshot levelSnapshot = null;
                if (!snapshotedLevels.Contains(levelEntity.Id))
                {
                    if (!LevelSnapshots.TryGetValue(levelEntity.Id, out levelSnapshot))
                    {
                        levelSnapshot = new LevelSnapshot();
                        LevelSnapshots[levelEntity.Id] = levelSnapshot;
                    }

                    levelSnapshot.Snapshot(levelEntity, context);
                    snapshotedLevels.Add(levelEntity.Id);
                }

                if (!PlayerSnapshots.TryGetValue(playerEntity.Id, out var playerSnapshot))
                {
                    playerSnapshot = new PlayerSnapshot();
                    PlayerSnapshots[playerEntity.Id] = playerSnapshot;
                }

                playerSnapshot.Snapshot(playerEntity, context);
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

            if (state == EntityState.Added)
            {
                var serializedLevel = LevelSnapshot.Serialize(levelEntity, null, null, context);
                return PlayerSnapshot.Serialize(playerEntity, state, null, serializedLevel, context);
            }
            else
            {
                if (!snapshot.SerializedLevels.TryGetValue(levelEntity.Id, out var serializedLevel))
                {
                    var position = playerEntity.Position;
                    var level = context.DbContext.Entry(position).Property(p => p.LevelId);
                    serializedLevel = LevelSnapshot.Serialize
                    (position.LevelEntity, level.IsModified
                            ? EntityState.Added
                            : state,
                        snapshot.LevelSnapshots.GetValueOrDefault(levelEntity.Id), context);
                    snapshot.SerializedLevels[levelEntity.Id] = serializedLevel;
                }

                return PlayerSnapshot.Serialize(
                    playerEntity, state, snapshot.PlayerSnapshots[playerEntity.Id], serializedLevel, context);
            }
        }
    }
}
