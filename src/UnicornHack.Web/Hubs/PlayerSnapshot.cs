using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Primitives;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Utils;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Hubs
{
    public class PlayerSnapshot
    {
        public int SnapshotTick { get; set; }
        private HashSet<LogEntry> LogEntriesSnapshot { get; } = new HashSet<LogEntry>(10);

        private HashSet<GameEntity> RacesSnapshot { get; } =
            new HashSet<GameEntity>(EntityEqualityComparer<GameEntity>.Instance);

        private Dictionary<GameEntity, InventoryItemSnapshot> ItemsSnapshot { get; } =
            new Dictionary<GameEntity, InventoryItemSnapshot>(EntityEqualityComparer<GameEntity>.Instance);

        private HashSet<GameEntity> AbilitiesSnapshot { get; } =
            new HashSet<GameEntity>(EntityEqualityComparer<GameEntity>.Instance);

        public PlayerSnapshot Snapshot(GameEntity playerEntity, SerializationContext context)
        {
            SnapshotTick = playerEntity.Game.CurrentTick;

            var manager = playerEntity.Manager;
            var races = manager.RacesToBeingRelationship[playerEntity.Id].Values;
            RacesSnapshot.Clear();
            RacesSnapshot.AddRange(races);

            var items = manager.EntityItemsToContainerRelationship[playerEntity.Id];
            ItemsSnapshot.Clear();
            ItemsSnapshot.AddRange(items, i => new InventoryItemSnapshot().Snapshot(i, context));

            var abilities = GetSlottedAbilities(playerEntity, manager);
            AbilitiesSnapshot.Clear();
            AbilitiesSnapshot.AddRange(abilities);

            LogEntriesSnapshot.Clear();
            LogEntriesSnapshot.AddRange(GetLogEntries(playerEntity.Player));

            return this;
        }

        public static List<object> Serialize(
            GameEntity playerEntity, EntityState state, PlayerSnapshot snapshot, List<object> serializedLevel,
            SerializationContext context)
        {
            var manager = context.Manager;
            var player = playerEntity.Player;
            var being = playerEntity.Being;
            List<object> properties;
            if (state == EntityState.Added)
            {
                properties = new List<object>(14) {(int)state};

                properties.Add(player.EntityId);
                properties.Add(player.ProperName);
                properties.Add(player.Game.CurrentTick);
                properties.Add(serializedLevel);
                properties.Add(manager.RacesToBeingRelationship[playerEntity.Id].Values
                    .Select(r => RaceSnapshot.Serialize(r, null, context)).ToList());
                properties.Add(manager.EntityItemsToContainerRelationship[playerEntity.Id]
                    .Select(t => InventoryItemSnapshot.Serialize(t, null, null, context)).ToList());
                properties.Add(GetSlottedAbilities(playerEntity, manager)
                    .Select(a => AbilitySnapshot.Serialize(a, null, context)).ToList());
                // TODO: Group log entries for the same tick
                // TODO: Only send entries since last player turn
                properties.Add(GetLogEntries(player)
                    .Select(e => LogEntrySnapshot.Serialize(e, null, context)).ToList());
                properties.Add(player.NextActionTick);
                properties.Add(player.NextLevelXP);
                properties.Add(being.ExperiencePoints);
                properties.Add(being.HitPoints);
                properties.Add(being.HitPointMaximum);
                properties.Add(being.EnergyPoints);
                properties.Add(being.EnergyPointMaximum);
                return properties;
            }

            properties = new List<object>(3)
            {
                (int)state,
                snapshot.SnapshotTick,
                player.Game.CurrentTick
            };
            var playerEntry = context.DbContext.Entry(player);

            var i = 2;
            if (serializedLevel != null)
            {
                properties.Add(i);
                properties.Add(serializedLevel);
            }

            i++;
            var serializedRaces = GameTransmissionProtocol.Serialize(
                manager.RacesToBeingRelationship[playerEntity.Id].Values,
                snapshot.RacesSnapshot,
                RaceSnapshot.Serialize,
                context);
            if (serializedRaces.Count > 0)
            {
                properties.Add(i);
                properties.Add(serializedRaces);
            }

            i++;
            var serializedItems = GameTransmissionProtocol.Serialize(
                manager.EntityItemsToContainerRelationship[playerEntity.Id],
                snapshot.ItemsSnapshot,
                InventoryItemSnapshot.Serialize,
                context);
            if (serializedItems.Count > 0)
            {
                properties.Add(i);
                properties.Add(serializedItems);
            }

            i++;
            var serializedAbilities = GameTransmissionProtocol.Serialize(
                GetSlottedAbilities(playerEntity, manager),
                snapshot.AbilitiesSnapshot,
                AbilitySnapshot.Serialize,
                context);
            if (serializedAbilities.Count > 0)
            {
                properties.Add(i);
                properties.Add(serializedAbilities);
            }

            i++;
            var serializedLog = GameTransmissionProtocol.Serialize(
                GetLogEntries(player),
                snapshot.LogEntriesSnapshot,
                LogEntrySnapshot.Serialize,
                context);
            if (serializedLog.Count > 0)
            {
                properties.Add(i);
                properties.Add(serializedLog);
            }

            i++;
            var nextActionTick = playerEntry.Property(nameof(PlayerComponent.NextActionTick));
            if (nextActionTick.IsModified)
            {
                properties.Add(i);
                properties.Add(player.NextActionTick);
            }

            i++;
            var nextLevelXP = playerEntry.Property(nameof(PlayerComponent.NextLevelXP));
            if (nextLevelXP.IsModified)
            {
                properties.Add(i);
                properties.Add(player.NextLevelXP);
            }

            var beingEntry = context.DbContext.Entry(being);
            if (beingEntry.State != EntityState.Unchanged)
            {
                i++;
                var xp = beingEntry.Property(nameof(BeingComponent.ExperiencePoints));
                if (xp.IsModified)
                {
                    properties.Add(i);
                    properties.Add(being.ExperiencePoints);
                }

                i++;
                var hitPoints = beingEntry.Property(nameof(BeingComponent.HitPoints));
                if (hitPoints.IsModified)
                {
                    properties.Add(i);
                    properties.Add(being.HitPoints);
                }

                i++;
                var hitPointMaximum = beingEntry.Property(nameof(BeingComponent.HitPointMaximum));
                if (hitPointMaximum.IsModified)
                {
                    properties.Add(i);
                    properties.Add(being.HitPointMaximum);
                }

                i++;
                var energyPoints = beingEntry.Property(nameof(BeingComponent.EnergyPoints));
                if (energyPoints.IsModified)
                {
                    properties.Add(i);
                    properties.Add(being.EnergyPoints);
                }

                i++;
                var energyPointMaximum = beingEntry.Property(nameof(BeingComponent.EnergyPointMaximum));
                if (energyPointMaximum.IsModified)
                {
                    properties.Add(i);
                    properties.Add(being.EnergyPointMaximum);
                }
            }

            return properties;
        }

        private static IEnumerable<LogEntry> GetLogEntries(PlayerComponent player)
            => player.LogEntries.OrderBy(e => e, LogEntry.Comparer)
                .Skip(Math.Max(0, player.LogEntries.Count - 10));

        private static IEnumerable<GameEntity> GetSlottedAbilities(GameEntity playerEntity, GameManager manager)
            => manager.AbilitiesToAffectableRelationship[playerEntity.Id]
                .Select(a => a.Ability)
                .Where(a => a.Slot != null)
                .Select(a => a.Entity);
    }
}
