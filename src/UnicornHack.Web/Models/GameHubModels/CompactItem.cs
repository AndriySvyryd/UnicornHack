using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using UnicornHack.Data;
using UnicornHack.Data.Properties;
using UnicornHack.Utils;

// ReSharper disable RedundantAssignment
namespace UnicornHack.Models.GameHubModels
{
    public static class CompactItem
    {
        private const int ItemPropertyCount = 8;

        public static List<object> Serialize(Item item, EntityState? state, SerializationContext context)
        {
            List<object> properties;
            switch (state)
            {
                case null:
                case EntityState.Added:
                    switch (item)
                    {
                        case Gold gold:
                            properties = state == null
                                ? new List<object>(ItemPropertyCount + 1)
                                : new List<object>(ItemPropertyCount + 2) {state};
                            return Serialize(gold, properties, context);
                        case Container container:
                            properties = state == null
                                ? new List<object>(ItemPropertyCount + 1)
                                : new List<object>(ItemPropertyCount + 2) {state};
                            return Serialize(container, properties, context);
                        default:
                            properties = state == null
                                ? new List<object>(ItemPropertyCount)
                                : new List<object>(ItemPropertyCount + 1) {state};
                            return Serialize(item, properties, context);
                    }

                case EntityState.Deleted:
                    return new List<object> {state, item.Id};
            }

            properties = new List<object>(2) {state};
            var itemEntry = context.Context.Entry(item);
            switch (item)
            {
                case Gold gold:
                    properties = Serialize(itemEntry, gold, properties, context);
                    break;
                case Container container:
                    properties = Serialize(itemEntry, container, properties, context);
                    break;
                default:
                    properties = Serialize(itemEntry, item, properties, context);
                    break;
            }

            return properties.Count > 2 ? properties : null;
        }

        private static List<object> Serialize(Item item, List<object> properties, SerializationContext context)
        {
            properties.Add(item.Id);
            properties.Add(item.BaseName);

            var slots = item.GetEquipableSlots(context.Observer.GetProperty<int>(PropertyData.Size.Name))
                .GetNonRedundantFlags(removeComposites: true)
                .Select(s => new object[] {(int)s, context.Services.Language.ToString(s, item.Actor, abbreviate: true)})
                .ToList();
            properties.Add(slots.Count > 0 ? slots : null);
            properties.Add(context.Services.Language.ToString(item));
            properties.Add(item.LevelX);
            properties.Add(item.LevelY);
            properties.Add((int)item.Type);
            properties.Add(item.EquippedSlot == null
                ? null
                : context.Services.Language.ToString(item.EquippedSlot.Value, item.Actor, abbreviate: true));

            return properties;
        }

        public static List<object> Serialize(
            EntityEntry itemEntry, Item item, List<object> properties, SerializationContext context)
        {
            properties.Add(item.Id);

            var observer = context.Observer;
            var sizeChanges = CollectionChanges.SerializeCollection(
                observer.Properties, CompactProperty.Serialize, p => p.Name == PropertyData.Size.Name, context);

            var i = 2;
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

            i++;
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

            if (itemEntry.State != EntityState.Unchanged)
            {
                i++;
                var levelX = itemEntry.Property(nameof(Item.LevelX));
                if (levelX.IsModified)
                {
                    properties.Add(i);
                    properties.Add(item.LevelX);
                }

                i++;
                var levelY = itemEntry.Property(nameof(Item.LevelY));
                if (levelY.IsModified)
                {
                    properties.Add(i);
                    properties.Add(item.LevelY);
                }

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

            return properties;
        }

        public static void Snapshot(Item item)
        {
            if (item is Container container)
            {
                container.Items.CreateSnapshot();
                foreach (var containedItem in container.Items)
                {
                    Snapshot(containedItem);
                }
            }
        }
    }
}