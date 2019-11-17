using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Data.Abilities;
using UnicornHack.Generation;
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

        private HashSet<GameEntity> AbilitiesSnapshot { get; } =
            new HashSet<GameEntity>(EntityEqualityComparer<GameEntity>.Instance);

        public PlayerSnapshot Snapshot(GameEntity playerEntity, SerializationContext context)
        {
            SnapshotTick = playerEntity.Game.CurrentTick;

            var manager = playerEntity.Manager;
            var races = manager.RacesToBeingRelationship[playerEntity.Id].Values;
            RacesSnapshot.Clear();
            RacesSnapshot.AddRange(races);

            var abilities = GetSlottedAbilities(playerEntity, manager);
            AbilitiesSnapshot.Clear();
            AbilitiesSnapshot.AddRange(abilities);

            LogEntriesSnapshot.Clear();
            LogEntriesSnapshot.AddRange(GetLogEntries(playerEntity.Player));

            return this;
        }

        public static List<object> Serialize(
            GameEntity playerEntity,
            EntityState state,
            PlayerSnapshot snapshot,
            List<object> serializedLevel,
            SerializationContext context)
        {
            var manager = context.Manager;
            var player = playerEntity.Player;
            var being = playerEntity.Being;
            List<object> properties;
            if (state == EntityState.Added)
            {
                properties = new List<object>(12) {(int)state};

                properties.Add(player.ProperName);
                properties.Add(player.Game.CurrentTick);
                properties.Add(serializedLevel);
                properties.Add(manager.RacesToBeingRelationship[playerEntity.Id].Values
                    .Select(r => RaceSnapshot.Serialize(r, null, context)).ToList());
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
                properties.Add(being.ReservedEnergyPoints);
                return properties;
            }

            properties = new List<object>(3)
            {
                (int)state,
                snapshot.SnapshotTick,
                player.Game.CurrentTick
            };
            var playerEntry = context.DbContext.Entry(player);

            var i = 1;
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

                i++;
                var reservedEnergyPoints = beingEntry.Property(nameof(BeingComponent.ReservedEnergyPoints));
                if (reservedEnergyPoints.IsModified)
                {
                    properties.Add(i);
                    properties.Add(being.ReservedEnergyPoints);
                }
            }

            return properties;
        }

        public static List<object> SerializeItems(GameEntity playerEntity, SerializationContext context)
        {
            var manager = context.Manager;
            return new List<object>(1)
            {
                manager.EntityItemsToContainerRelationship[playerEntity.Id]
                    .Select(t => InventoryItemSnapshot.Serialize(t, null, null, context)).ToList()
            };
        }

        public static List<object> SerializeAdaptations(GameEntity playerEntity, SerializationContext context)
        {
            var traits = new List<(string, int)>();
            var mutations = new List<(string, int)>();
            foreach (var effectEntity in context.Manager.AppliedEffectsToAffectableEntityRelationship[playerEntity.Id])
            {
                var effect = effectEntity.Effect;
                if (effect.EffectType != EffectType.AddAbility
                    || effect.TargetName == null
                    || !(Ability.Loader.Find(effect.TargetName) is LeveledAbility template))
                {
                    continue;
                }

                switch (template.Type)
                {
                    case AbilityType.Trait:
                        traits.Add((template.Name, effect.Amount.Value));
                        break;
                    case AbilityType.Mutation:
                        mutations.Add((template.Name, effect.Amount.Value));
                        break;
                    default:
                        continue;
                }
            }

            return new List<object>(4)
            {
                playerEntity.Player.TraitPoints,
                playerEntity.Player.MutationPoints,
                traits,
                mutations
            };
        }

        public static List<object> SerializeSkills(GameEntity playerEntity, SerializationContext context)
        {
            var manager = context.Manager;
            var playerId = playerEntity.Id;

            return new List<object>(31)
            {
                playerEntity.Player.SkillPoints,
                GetAbilityLevel(nameof(AbilityData.HandWeapons), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.ShortWeapons), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.MediumWeapons), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.LongWeapons), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.CloseRangeWeapons), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.ShortRangeWeapons), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.MediumRangeWeapons), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.LongRangeWeapons), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.OneHanded), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.TwoHanded), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.DualWielding), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.Acrobatics), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.LightArmor), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.HeavyArmor), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.AirSourcery), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.BloodSourcery), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.EarthSourcery), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.FireSourcery), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.SpiritSourcery), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.WaterSourcery), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.Conjuration), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.Enchantment), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.Evocation), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.Malediction), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.Illusion), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.Transmutation), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.Assassination), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.Stealth), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.Artifice), playerId, manager),
                GetAbilityLevel(nameof(AbilityData.Leadership), playerId, manager)
            };
        }

        private static int GetAbilityLevel(string abilityName, int playerId, GameManager manager)
            => manager.AffectableAbilitiesIndex[(playerId, abilityName)]?.Ability.Level ?? 0;

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
