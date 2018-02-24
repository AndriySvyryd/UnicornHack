using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Data;
using UnicornHack.Data.Properties;
using UnicornHack.Utils;

// ReSharper disable RedundantAssignment
namespace UnicornHack.Models.GameHubModels
{
    public static class CompactItem
    {
        public static List<object> Serialize(Item item, EntityState? state, SerializationContext context)
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
                observer.Properties, CompactProperty.Serialize, p => p.Name == PropertyData.Size.Name, context);
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

        public static List<object> Serialize(ItemKnowledge itemKnowledge, EntityState? state,
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
    }
}