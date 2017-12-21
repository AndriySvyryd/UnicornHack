using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using UnicornHack.Abilities;
using UnicornHack.Data;
using UnicornHack.Effects;

// ReSharper disable RedundantAssignment
namespace UnicornHack.Models.GameHubModels
{
    public static class CompactActor
    {
        private const int ActorPropertyCount = 6;

        public static List<object> Serialize(Actor actor, EntityState? state, SerializationContext context)
        {
            List<object> properties;
            switch (state)
            {
                case null:
                case EntityState.Added:
                    switch (actor)
                    {
                        case Player player:
                            properties = state == null
                                ? new List<object>(ActorPropertyCount + 7)
                                : new List<object>(ActorPropertyCount + 8) {state};
                            return Serialize(player, properties, context);
                        default:
                            properties = state == null
                                ? new List<object>(ActorPropertyCount)
                                : new List<object>(ActorPropertyCount + 1) {state};
                            return Serialize(actor, properties);
                    }
                case EntityState.Deleted:
                    return new List<object> {state, actor.Id};
            }

            properties = new List<object>(2) {state};
            var actorEntry = context.Context.Entry(actor);
            switch (actor)
            {
                case Player player:
                    properties = Serialize(actorEntry, player, properties, context);
                    break;
                default:
                    properties = Serialize(actorEntry, actor, properties);
                    break;
            }

            return properties.Count > 2 ? properties : null;
        }

        private static List<object> Serialize(Actor actor, List<object> properties)
        {
            properties.Add(actor.Id);
            properties.Add(actor.VariantName);
            properties.Add(actor.Name);
            properties.Add(actor.LevelX);
            properties.Add(actor.LevelY);
            properties.Add((byte)actor.Heading);

            return properties;
        }

        private static List<object> Serialize(Player player, List<object> properties, SerializationContext context)
        {
            properties = Serialize(player, properties);

            properties.Add(player.Properties
                .Select(p => CompactProperty.Serialize(p, null, context)).ToList());
            properties.Add(player.Inventory
                .Select(t => CompactItem.Serialize(t, null, context)).ToList());
            properties.Add(player.Abilities.Where(a => a.IsUsable && a.Activation == AbilityActivation.OnTarget)
                .Select(a => CompactAbility.Serialize(a, null, context)).ToList());
            // TODO: Group log entries for the same tick
            properties.Add(player.Log.OrderBy(e => e, LogEntry.Comparer).Skip(Math.Max(0, player.Log.Count - 10))
                .Select(e => CompactLogEntry.Serialize(e, null, context)).ToList());
            properties.Add(player.Races
                .Select(r => CompactPlayerRace.Serialize(r, null, context)).ToList());
            properties.Add(player.NextActionTick);
            properties.Add(player.Gold);

            return properties;
        }

        private static List<object> Serialize(
            EntityEntry actorEntry, Actor actor, List<object> properties)
        {
            properties.Add(actor.Id);

            var i = 2;
            if (actorEntry.State != EntityState.Unchanged)
            {
                var name = actorEntry.Property(nameof(Actor.Name));
                if (name.IsModified)
                {
                    properties.Add(i);
                    properties.Add(actor.Name);
                }

                i++;
                var levelX = actorEntry.Property(nameof(Actor.LevelX));
                if (levelX.IsModified)
                {
                    properties.Add(i);
                    properties.Add(actor.LevelX);
                }
                i++;
                var levelY = actorEntry.Property(nameof(Actor.LevelY));
                if (levelY.IsModified)
                {
                    properties.Add(i);
                    properties.Add(actor.LevelY);
                }

                i++;
                var heading = actorEntry.Property(nameof(Actor.Heading));
                if (heading.IsModified)
                {
                    properties.Add(i);
                    properties.Add((byte)actor.Heading);
                }
            }

            return properties;
        }

        private static List<object> Serialize(
            EntityEntry actorEntry, Player player, List<object> properties, SerializationContext context)
        {
            properties = Serialize(actorEntry, player, properties);

            var i = ActorPropertyCount;

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

            if (actorEntry.State != EntityState.Unchanged)
            {
                i++;
                var nextActionTick = actorEntry.Property(nameof(Player.NextActionTick));
                if (nextActionTick.IsModified)
                {
                    properties.Add(i);
                    properties.Add(player.NextActionTick);
                }

                i++;
                var gold = actorEntry.Property(nameof(Player.Gold));
                if (gold.IsModified)
                {
                    properties.Add(i);
                    properties.Add(player.Gold);
                }
            }

            return properties;
        }

        public static void Snapshot(Actor actor)
        {
            if (actor is Player player)
            {
                player.Properties.CreateSnapshot();

                player.Inventory.CreateSnapshot();
                foreach (var item in player.Inventory)
                {
                    CompactItem.Snapshot(item);
                }

                player.Abilities.CreateSnapshot();
                foreach (var ability in player.Abilities)
                {
                    CompactAbility.Snapshot(ability);
                }

                player.Log.CreateSnapshot();

                player.ActiveEffects.CreateSnapshot();
            }
        }
    }
}