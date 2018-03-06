using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Abilities;
using UnicornHack.Data;
using UnicornHack.Data.Properties;
using UnicornHack.Effects;
using UnicornHack.Utils;

namespace UnicornHack.Hubs
{
    public class GameTransmissionProtocol
    {
        public List<object> Serialize(Player player, EntityState state, SerializationContext context)
        {
            List<object> properties;
            switch (state)
            {
                case EntityState.Added:
                    properties = new List<object>(12) {(int)state};

                    properties.Add(player.Id);
                    properties.Add(player.Name);
                    properties.Add(player.Level.CurrentTick);
                    properties.Add(Serialize(player.Level, null, context));
                    properties.Add(player.Properties
                        .Select(p => Serialize(p, null, context)).ToList());
                    properties.Add(player.Inventory
                        .Select(t => Serialize(t, null, context)).ToList());
                    properties.Add(player.Abilities.Where(a => a.IsUsable && a.Activation == AbilityActivation.OnTarget)
                        .Select(a => Serialize(a, null, context)).ToList());
                    // TODO: Group log entries for the same tick
                    // TODO: Only send entries since last player turn
                    properties.Add(player.Log.OrderBy(e => e, LogEntry.Comparer)
                        .Skip(Math.Max(0, player.Log.Count - 10))
                        .Select(e => Serialize(e, null, context)).ToList());
                    properties.Add(player.Races
                        .Select(r => Serialize(r, null, context)).ToList());
                    properties.Add(player.NextActionTick);
                    properties.Add(player.Gold);
                    return properties;
            }

            properties = new List<object>(3) {(int)state, player.SnapshotTick, player.Level.CurrentTick};
            var playerEntry = context.Context.Entry(player);

            var i = 2;
            var level = playerEntry.Reference(p => p.Level);
            var serializedLevel = Serialize(player.Level, level.IsModified ? EntityState.Added : state, context);
            if (serializedLevel != null)
            {
                properties.Add(i);
                properties.Add(serializedLevel);
            }

            i++;
            var serializedProperties = CollectionChanges
                .SerializeCollection(player.Properties, Serialize, context);
            if (serializedProperties.Count > 0)
            {
                properties.Add(i);
                properties.Add(serializedProperties);
            }

            i++;
            var serializedItems = CollectionChanges
                .SerializeCollection(player.Inventory, Serialize, context);
            if (serializedItems.Count > 0)
            {
                properties.Add(i);
                properties.Add(serializedItems);
            }

            i++;
            var serializedAbilities = CollectionChanges
                .SerializeCollection(player.Abilities, Serialize,
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
                    Serialize,
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
                player.ActiveEffects, Serialize, (AppliedEffect e) => e is ChangedRace, context);
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

        public List<object> Serialize(Level level, EntityState? state, SerializationContext context)
        {
            List<object> properties;
            switch (state)
            {
                case null:
                case EntityState.Added:
                    var knownTerrain = new List<short>();
                    for (short j = 0; j < level.KnownTerrain.Length; j++)
                    {
                        var feature = level.KnownTerrain[j];
                        if (feature != (byte)MapFeature.Unexplored)
                        {
                            knownTerrain.Add(j);
                            knownTerrain.Add(feature);
                        }
                    }

                    var wallNeighbours = new List<short>();
                    for (short j = 0; j < level.WallNeighbours.Length; j++)
                    {
                        if (level.KnownTerrain[j] == (byte)MapFeature.Unexplored)
                        {
                            continue;
                        }

                        var neighbours = level.WallNeighbours[j] & (byte)DirectionFlags.Cross;
                        if (neighbours != (byte)DirectionFlags.None)
                        {
                            wallNeighbours.Add(j);
                            wallNeighbours.Add((byte)neighbours);
                        }
                    }

                    var visibleTerrain = new List<short>();
                    for (short j = 0; j < level.VisibleTerrain.Length; j++)
                    {
                        var visibility = level.VisibleTerrain[j];
                        if (visibility != 0)
                        {
                            visibleTerrain.Add(j);
                            visibleTerrain.Add(visibility);
                        }
                    }

                    properties = state == null
                        ? new List<object>(10)
                        : new List<object>(11) {(int)state};

                    properties.Add(level.ActorsKnowledge.Select(a => Serialize(a, null, context))
                        .ToList());
                    properties.Add(level.ItemsKnowledge.Select(t => Serialize(t, null, context)).ToList());
                    properties.Add(level.Connections.Where(c => c.Known)
                        .Select(c => Serialize(c, null, context)).ToList());
                    properties.Add(knownTerrain);
                    properties.Add(wallNeighbours);
                    properties.Add(visibleTerrain);
                    properties.Add(level.BranchName);
                    properties.Add(level.Depth);
                    properties.Add(level.Width);
                    properties.Add(level.Height);
                    return properties;
            }

            var dbContext = context.Context;
            var levelEntry = dbContext.Entry(level);
            properties = new List<object> {(int)state};

            var i = 2;
            var serializedActors = CollectionChanges
                .SerializeCollection(level.ActorsKnowledge, Serialize, context);
            if (serializedActors.Count > 0)
            {
                properties.Add(i);
                properties.Add(serializedActors);
            }

            i++;
            var serializedItems = CollectionChanges
                .SerializeCollection(level.ItemsKnowledge, Serialize, context);
            if (serializedItems.Count > 0)
            {
                properties.Add(i);
                properties.Add(serializedItems);
            }

            i++;
            var serializedConnections = CollectionChanges
                .SerializeCollection(level.Connections, Serialize,
                    v => (bool)v[nameof(Connection.Known)], context);
            if (serializedConnections.Count > 0)
            {
                properties.Add(i);
                properties.Add(serializedConnections);
            }

            if (levelEntry.State != EntityState.Unchanged)
            {
                i++;
                var wallNeighboursChanges = new List<object>();
                var knownTerrainChanges = new List<object>(level.KnownTerrainChanges.Count * 2);
                if (level.KnownTerrainChanges.Count > 0)
                {
                    foreach (var terrainChange in level.KnownTerrainChanges)
                    {
                        knownTerrainChanges.Add(terrainChange.Key);
                        knownTerrainChanges.Add(terrainChange.Value);

                        wallNeighboursChanges.Add(terrainChange.Key);
                        wallNeighboursChanges.Add(level.WallNeighbours[terrainChange.Key] & (byte)DirectionFlags.Cross);
                    }
                }

                if (level.TerrainChanges.Count > 0)
                {
                    foreach (var terrainChange in level.TerrainChanges)
                    {
                        if (level.VisibleTerrain[terrainChange.Key] == 0)
                        {
                            continue;
                        }

                        knownTerrainChanges.Add(terrainChange.Key);
                        knownTerrainChanges.Add(terrainChange.Value);
                    }
                }

                if (knownTerrainChanges.Count > 0)
                {
                    properties.Add(i);
                    properties.Add(knownTerrainChanges);
                }

                i++;
                if (level.WallNeighboursChanges.Count > 0)
                {
                    foreach (var wallNeighboursChange in level.WallNeighboursChanges)
                    {
                        if (level.VisibleTerrain[wallNeighboursChange.Key] == 0)
                        {
                            continue;
                        }

                        wallNeighboursChanges.Add(wallNeighboursChange.Key);
                        wallNeighboursChanges.Add(wallNeighboursChange.Value & (byte)DirectionFlags.Cross);
                    }
                }

                if (wallNeighboursChanges.Count > 0)
                {
                    properties.Add(i);
                    properties.Add(wallNeighboursChanges);
                }

                i++;
                if (level.VisibleTerrainChanges.Count > 0)
                {
                    properties.Add(i);
                    var changes = new object[level.VisibleTerrainChanges.Count * 2];
                    var j = 0;
                    foreach (var visibleTerrainChange in level.VisibleTerrainChanges)
                    {
                        changes[j++] = visibleTerrainChange.Key;
                        changes[j++] = visibleTerrainChange.Value;
                    }

                    properties.Add(changes);
                }
            }

            return properties.Count > 1 ? properties : null;
        }

        public List<object> Serialize(AppliedEffect raceEffect, EntityState? state, SerializationContext context)
        {
            var race = (ChangedRace)raceEffect;
            List<object> properties;
            switch (state)
            {
                case null:
                case EntityState.Added:
                {
                    properties = state == null
                        ? new List<object>(5)
                        : new List<object>(6) {(int)state};
                    properties.Add(race.Id);
                    properties.Add(race.Name);
                    properties.Add(race.XPLevel);
                    properties.Add(race.XP);
                    properties.Add(race.NextLevelXP);
                    return properties;
                }
                case EntityState.Deleted:
                    return new List<object> {(int)state, race.Id};
            }

            properties = new List<object> {(int)state, race.Id};

            var raceEntry = context.Context.Entry(race);
            var i = 1;

            if (raceEntry.State != EntityState.Unchanged)
            {
                var name = raceEntry.Property(nameof(ChangedRace.Name));
                if (name.IsModified)
                {
                    properties.Add(i);
                    properties.Add(race.Name);
                }

                i++;
                var xpLevel = raceEntry.Property(nameof(ChangedRace.XPLevel));
                if (xpLevel.IsModified)
                {
                    properties.Add(i);
                    properties.Add(race.XPLevel);
                }

                i++;
                var xp = raceEntry.Property(nameof(ChangedRace.XP));
                if (xp.IsModified)
                {
                    properties.Add(i);
                    properties.Add(race.XP);
                }

                i++;
                var nextLevelXP = raceEntry.Property(nameof(ChangedRace.NextLevelXP));
                if (nextLevelXP.IsModified)
                {
                    properties.Add(i);
                    properties.Add(race.NextLevelXP);
                }
            }

            return properties.Count > 2 ? properties : null;
        }

        public List<object> Serialize(ActorKnowledge actorKnowledge, EntityState? state, SerializationContext context)
        {
            List<object> properties;
            switch (state)
            {
                case null:
                case EntityState.Added:
                    properties = state == null
                        ? new List<object>(6)
                        : new List<object>(7) {(int)state};
                    properties.Add(actorKnowledge.Id);
                    properties.Add(actorKnowledge.Actor.VariantName);
                    properties.Add(actorKnowledge.Actor.Name);
                    properties.Add(actorKnowledge.LevelX);
                    properties.Add(actorKnowledge.LevelY);
                    properties.Add((byte)actorKnowledge.Heading);
                    return properties;
                case EntityState.Deleted:
                    return new List<object> {(int)state, actorKnowledge.Id};
            }

            properties = new List<object>(2) {(int)state, actorKnowledge.Id};

            var actorKnowledgeEntry = context.Context.Entry(actorKnowledge);
            var actorEntry = context.Context.Entry(actorKnowledge.Actor);
            var i = 2;
            if (actorKnowledgeEntry.State != EntityState.Unchanged)
            {
                var name = actorEntry.Property(nameof(Actor.Name));
                if (name.IsModified)
                {
                    properties.Add(i);
                    properties.Add(actorKnowledge.Actor.Name);
                }

                i++;
                var levelX = actorKnowledgeEntry.Property(nameof(Actor.LevelX));
                if (levelX.IsModified)
                {
                    properties.Add(i);
                    properties.Add(actorKnowledge.LevelX);
                }

                i++;
                var levelY = actorKnowledgeEntry.Property(nameof(Actor.LevelY));
                if (levelY.IsModified)
                {
                    properties.Add(i);
                    properties.Add(actorKnowledge.LevelY);
                }

                i++;
                var heading = actorKnowledgeEntry.Property(nameof(Actor.Heading));
                if (heading.IsModified)
                {
                    properties.Add(i);
                    properties.Add((byte)actorKnowledge.Heading);
                }
            }

            return properties.Count > 2 ? properties : null;
        }

        public List<object> Serialize(Item item, EntityState? state, SerializationContext context)
        {
            List<object> properties;
            switch (state)
            {
                case null:
                case EntityState.Added:
                    properties = state == null
                        ? new List<object>(6)
                        : new List<object>(7) {(int)state};
                    properties.Add(item.Id);
                    properties.Add((int)item.Type);
                    properties.Add(item.VariantName);
                    properties.Add(context.Services.Language.ToString(item));
                    var slots = item.GetEquipableSlots(context.Observer.GetProperty<int>(PropertyData.Size.Name))
                        .GetNonRedundantFlags(removeComposites: true)
                        .Select(s => new object[]
                            {(int)s, context.Services.Language.ToString(s, item.Actor, abbreviate: true)})
                        .ToList();
                    properties.Add(slots.Count > 0 ? slots : null);
                    properties.Add(item.EquippedSlot == null
                        ? null
                        : context.Services.Language.ToString(item.EquippedSlot.Value, item.Actor, abbreviate: true));
                    return properties;
                case EntityState.Deleted:
                    return new List<object> {(int)state, item.Id};
            }

            var itemEntry = context.Context.Entry(item);
            properties = new List<object>(2) {(int)state, item.Id};

            var i = 3;
            var name = itemEntry.Property(nameof(Item.Name));
            var quantityModified = false;
            if (item is Gold)
            {
                quantityModified = itemEntry.Property(nameof(Gold.Quantity)).IsModified;
            }
            else if (item is Container container)
            {
                var itemsChanges = CollectionChanges.SerializeCollection(
                    container.Items, Serialize, context);

                quantityModified = itemsChanges.Count > 0;
            }

            if (name.IsModified
                || quantityModified)
            {
                properties.Add(i);
                properties.Add(context.Services.Language.ToString(item));
            }

            i++;
            var observer = context.Observer;
            var sizeChanges = CollectionChanges.SerializeCollection(
                observer.Properties, Serialize, p => p.Name == PropertyData.Size.Name, context);
            if (sizeChanges.Count > 0)
            {
                var slots = item.GetEquipableSlots(observer.GetProperty<int>(PropertyData.Size.Name))
                    .GetNonRedundantFlags(removeComposites: true)
                    .Select(s => new List<object>
                    {
                        (int)s,
                        context.Services.Language.ToString(s, item.Actor, abbreviate: true)
                    })
                    .ToList();

                properties.Add(i);
                properties.Add(slots);
            }

            if (itemEntry.State != EntityState.Unchanged)
            {
                i++;
                var equippedSlot = itemEntry.Property(nameof(Item.EquippedSlot));
                if (equippedSlot.IsModified)
                {
                    properties.Add(i);
                    properties.Add(item.EquippedSlot == null
                        ? null
                        : context.Services.Language.ToString(item.EquippedSlot.Value, item.Actor, abbreviate: true));
                }
            }

            return properties.Count > 2 ? properties : null;
        }

        public List<object> Serialize(ItemKnowledge itemKnowledge, EntityState? state,
            SerializationContext context)
        {
            List<object> properties;
            switch (state)
            {
                case null:
                case EntityState.Added:
                    properties = state == null
                        ? new List<object>(6)
                        : new List<object>(7) {(int)state};
                    properties.Add(itemKnowledge.Id);
                    properties.Add((int)itemKnowledge.Item.Type);
                    properties.Add(itemKnowledge.Item.VariantName);
                    properties.Add(context.Services.Language.ToString(itemKnowledge.Item));
                    properties.Add(itemKnowledge.LevelX);
                    properties.Add(itemKnowledge.LevelY);
                    return properties;
                case EntityState.Deleted:
                    return new List<object> {(int)state, itemKnowledge.Id};
            }

            properties = new List<object>(2) {(int)state, itemKnowledge.Id};
            var itemKnowledgeEntry = context.Context.Entry(itemKnowledge);
            var itemEntry = context.Context.Entry(itemKnowledge.Item);

            var i = 3;
            var name = itemEntry.Property(nameof(Item.Name));
            var quantityModified = false;
            if (itemKnowledge.Item is Gold)
            {
                quantityModified = itemEntry.Property(nameof(Gold.Quantity)).IsModified;
            }
            else if (itemKnowledge.Item is Container container)
            {
                var itemsChanges = CollectionChanges.SerializeCollection(
                    container.Items, Serialize, context);

                quantityModified = itemsChanges.Count > 0;
            }

            if (name.IsModified
                || quantityModified)
            {
                properties.Add(i);
                properties.Add(context.Services.Language.ToString(itemKnowledge.Item));
            }

            if (itemKnowledgeEntry.State != EntityState.Unchanged)
            {
                i++;
                var levelX = itemKnowledgeEntry.Property(nameof(Item.LevelX));
                if (levelX.IsModified)
                {
                    properties.Add(i);
                    properties.Add(itemKnowledge.LevelX);
                }

                i++;
                var levelY = itemKnowledgeEntry.Property(nameof(Item.LevelY));
                if (levelY.IsModified)
                {
                    properties.Add(i);
                    properties.Add(itemKnowledge.LevelY);
                }
            }

            return properties.Count > 2 ? properties : null;
        }

        public List<object> Serialize(Connection connection, EntityState? state, SerializationContext context)
        {
            List<object> properties;
            switch (state)
            {
                case null:
                case EntityState.Added:
                    properties = state == null
                        ? new List<object>(4)
                        : new List<object>(5) {(int)state};
                    properties.Add(connection.Id);
                    properties.Add(connection.LevelX);
                    properties.Add(connection.LevelY);
                    properties.Add(connection.TargetLevelDepth > connection.LevelDepth);
                    return properties;
                case EntityState.Deleted:
                    return new List<object> {(int)state, connection.Id};
            }

            var connectionEntry = context.Context.Entry(connection);
            properties = new List<object> {(int)state, connection.Id};

            var i = 1;
            if (connectionEntry.State != EntityState.Unchanged)
            {
                var levelX = connectionEntry.Property(nameof(Connection.LevelX));
                if (levelX.IsModified)
                {
                    properties.Add(i);
                    properties.Add(connection.LevelX);
                }

                i++;
                var levelY = connectionEntry.Property(nameof(Connection.LevelY));
                if (levelY.IsModified)
                {
                    properties.Add(i);
                    properties.Add(connection.LevelY);
                }
            }

            return properties.Count > 2 ? properties : null;
        }

        public List<object> Serialize(Property property, EntityState? state, SerializationContext context)
        {
            List<object> properties;
            switch (state)
            {
                case null:
                case EntityState.Added:
                    properties = state == null
                        ? new List<object>(2)
                        : new List<object>(3) {(int)state};

                    properties.Add(property.Name);
                    properties.Add(context.Services.Language.ToString(property));
                    return properties;
                case EntityState.Deleted:
                    return new List<object> {(int)state, property.Name};
            }

            properties = new List<object> {(int)state, property.Name};
            var propertyEntry = context.Context.Entry(property);
            var i = 1;

            if (propertyEntry.State != EntityState.Unchanged)
            {
                var value = propertyEntry.Property("_currentValue");
                if (value.IsModified)
                {
                    properties.Add(i);
                    properties.Add(context.Services.Language.ToString(property));
                }
            }

            return properties.Count > 2 ? properties : null;
        }

        public List<object> Serialize(Ability ability, EntityState? state, SerializationContext context)
        {
            List<object> properties;
            switch (state)
            {
                case null:
                case EntityState.Added:
                {
                    var canBeDefault = CanBeDefault(ability);
                    {
                        properties = state == null
                            ? new List<object>(canBeDefault ? 3 : 2)
                            : new List<object>(canBeDefault ? 4 : 3) {(int)state};
                    }

                    properties.Add(ability.Id);
                    properties.Add(context.Services.Language.ToString(ability));
                    if (canBeDefault
                        && ability.Entity is Player addedPlayer)
                    {
                        properties.Add(addedPlayer.DefaultAttack == ability);
                    }

                    return properties;
                }
                case EntityState.Deleted:
                    return new List<object> {(int)state, ability.Id};
            }

            properties = new List<object> {(int)state, ability.Id};
            var abilityEntry = context.Context.Entry(ability);
            var i = 1;
            var name = abilityEntry.Property(nameof(Ability.Name));
            if (name.IsModified)
            {
                properties.Add(i);
                properties.Add(context.Services.Language.ToString(ability));
            }

            i++;
            if (CanBeDefault(ability)
                && ability.Entity is Player player)
            {
                var defaultAttack = abilityEntry.Context.Entry(player).Reference(nameof(Player.DefaultAttack));
                if (defaultAttack.IsModified)
                {
                    properties.Add(i);
                    properties.Add(player.DefaultAttack == ability);
                }
            }

            return properties.Count > 2 ? properties : null;
        }

        private bool CanBeDefault(Ability ability)
        {
            switch (ability.Name)
            {
                case Actor.DoubleMeleeAttackName:
                case Actor.PrimaryMeleeAttackName:
                case Actor.SecondaryMeleeAttackName:
                case Actor.DoubleRangedAttackName:
                case Actor.PrimaryRangedAttackName:
                case Actor.SecondaryRangedAttackName:
                    return true;
                default:
                    return false;
            }
        }

        public List<object> Serialize(LogEntry entry, EntityState? state, SerializationContext context)
        {
            switch (state)
            {
                case null:
                    return new List<object>
                    {
                        entry.Id,
                        ToString(entry)
                    };
                case EntityState.Added:
                    return new List<object>
                    {
                        (int)state,
                        entry.Id,
                        ToString(entry)
                    };
                case EntityState.Deleted:
                    return new List<object>
                    {
                        (int)state,
                        entry.Id
                    };
            }

            var properties = new List<object> {(int)state, entry.Id};

            var logEntry = context.Context.Entry(entry);
            var i = 1;
            var tick = logEntry.Property(nameof(LogEntry.Tick));
            var message = logEntry.Property(nameof(LogEntry.Message));
            if (tick.IsModified
                || message.IsModified)
            {
                properties.Add(i);
                properties.Add(ToString(entry));
            }

            return properties.Count > 2 ? properties : null;
        }

        private string ToString(LogEntry entry) => $"{entry.Tick / 100f:0000.00}: {entry.Message}";
    }
}