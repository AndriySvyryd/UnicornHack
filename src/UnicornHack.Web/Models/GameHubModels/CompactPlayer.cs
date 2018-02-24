using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Abilities;
using UnicornHack.Data;
using UnicornHack.Effects;

// ReSharper disable RedundantAssignment
namespace UnicornHack.Models.GameHubModels
{
    public static class CompactPlayer
    {
        public static List<object> Serialize(Player player, EntityState state, SerializationContext context)
        {
            List<object> properties;
            switch (state)
            {
                case EntityState.Added:
                    properties = new List<object>(12) {(int)state};

                    properties.Add(player.Id);
                    properties.Add(player.Name);
                    properties.Add(player.Level.CurrentTick);
                    properties.Add(CompactLevel.Serialize(player.Level, null, context));
                    properties.Add(player.Properties
                        .Select(p => CompactProperty.Serialize(p, null, context)).ToList());
                    properties.Add(player.Inventory
                        .Select(t => CompactItem.Serialize(t, null, context)).ToList());
                    properties.Add(player.Abilities.Where(a => a.IsUsable && a.Activation == AbilityActivation.OnTarget)
                        .Select(a => CompactAbility.Serialize(a, null, context)).ToList());
                    // TODO: Group log entries for the same tick
                    // TODO: Only send entries since last player turn
                    properties.Add(player.Log.OrderBy(e => e, LogEntry.Comparer)
                        .Skip(Math.Max(0, player.Log.Count - 10))
                        .Select(e => CompactLogEntry.Serialize(e, null, context)).ToList());
                    properties.Add(player.Races
                        .Select(r => CompactPlayerRace.Serialize(r, null, context)).ToList());
                    properties.Add(player.NextActionTick);
                    properties.Add(player.Gold);
                    return properties;
            }

            properties = new List<object>(3) {(int)state, player.SnapshotTick, player.Level.CurrentTick};
            var playerEntry = context.Context.Entry(player);

            var i = 2;
            var level = playerEntry.Reference(p => p.Level);
            var serializedLevel = CompactLevel
                .Serialize(player.Level, level.IsModified ? EntityState.Added : state, context);
            if (serializedLevel != null)
            {
                properties.Add(i);
                properties.Add(serializedLevel);
            }

            i++;
            var serializedProperties = CollectionChanges
                .SerializeCollection(player.Properties, CompactProperty.Serialize, context);
            if (serializedProperties.Count > 0)
            {
                properties.Add(i);
                properties.Add(serializedProperties);
            }

            i++;
            var serializedItems = CollectionChanges
                .SerializeCollection(player.Inventory, CompactItem.Serialize, context);
            if (serializedItems.Count > 0)
            {
                properties.Add(i);
                properties.Add(serializedItems);
            }

            i++;
            var serializedAbilities = CollectionChanges
                .SerializeCollection(player.Abilities, CompactAbility.Serialize,
                    v => (bool)v[nameof(Ability.IsUsable)]
                         && (AbilityActivation)v[nameof(Ability.Activation)] == AbilityActivation.OnTarget,
                    context);
            if (serializedAbilities.Count > 0)
            {
                properties.Add(i);
                properties.Add(serializedAbilities);
            }

            i++;
            var serializedLog = CollectionChanges
                .SerializeCollection<LogEntry, List<LogEntry>, object>(
                    player.Log,
                    CompactLogEntry.Serialize,
                    (e, _) =>
                    {
                        var list = e.OrderBy(en => en, LogEntry.Comparer).ToList();
                        list.RemoveRange(0, Math.Max(0, list.Count - 10));
                        return list;
                    },
                    (e, _) => player.Log.OrderBy(en => en, LogEntry.Comparer).Skip(Math.Max(0, player.Log.Count - 10))
                        .ToList(),
                    (entry, list, _) => list.BinarySearch(entry, LogEntry.Comparer) > list.Count - 10,
                    context,
                    null);
            if (serializedLog.Count > 0)
            {
                properties.Add(i);
                properties.Add(serializedLog);
            }

            i++;
            var serializedRaces = CollectionChanges.SerializeCollection(
                player.ActiveEffects, CompactPlayerRace.Serialize, (AppliedEffect e) => e is ChangedRace, context);
            if (serializedRaces.Count > 0)
            {
                properties.Add(i);
                properties.Add(serializedRaces);
            }

            if (playerEntry.State != EntityState.Unchanged)
            {
                i++;
                var nextActionTick = playerEntry.Property(nameof(Player.NextActionTick));
                if (nextActionTick.IsModified)
                {
                    properties.Add(i);
                    properties.Add(player.NextActionTick);
                }

                i++;
                var gold = playerEntry.Property(nameof(Player.Gold));
                if (gold.IsModified)
                {
                    properties.Add(i);
                    properties.Add(player.Gold);
                }
            }

            return properties;
        }
    }
}