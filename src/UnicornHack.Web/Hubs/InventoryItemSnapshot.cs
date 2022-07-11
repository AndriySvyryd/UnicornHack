using Microsoft.EntityFrameworkCore;
using UnicornHack.Services;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Items;

namespace UnicornHack.Hubs;

public class InventoryItemSnapshot
{
    private string? NameSnapshot
    {
        get;
        set;
    }

    public InventoryItemSnapshot Snapshot(GameEntity itemEntity, SerializationContext context)
    {
        var item = itemEntity.Item!;
        NameSnapshot = context.Services.Language.GetString(item, item.GetQuantity(), SenseType.Sight);

        return this;
    }

    public static List<object?>? Serialize(
        GameEntity itemEntity, EntityState? state, InventoryItemSnapshot? snapshot, SerializationContext context)
    {
        List<object?> properties;
        switch (state)
        {
            case null:
            case EntityState.Added:
            {
                var item = itemEntity.Item!;
                var manager = context.Manager;
                properties = state == null
                    ? new List<object?>(6)
                    : new List<object?>(7) { (int)state };
                properties.Add(itemEntity.Id);
                properties.Add((int)item.Type);
                properties.Add(item.TemplateName);
                properties.Add(
                    context.Services.Language.GetString(item, item.GetQuantity(), SenseType.Sight));
                var slots = manager.ItemUsageSystem.GetEquipableSlots(item, context.Observer.Physical!.Size)
                    .GetNonRedundantFlags(removeComposites: true)
                    .Select(s => Serialize(s, context))
                    .ToList();
                properties.Add(slots.Count > 0 ? slots : null);
                properties.Add(item.EquippedSlot == EquipmentSlot.None
                    ? null
                    : Serialize(item.EquippedSlot, context));
                return properties;
            }
            case EntityState.Deleted:
                return new List<object?> { (int)state, itemEntity.Id };
            default:
            {
                var item = itemEntity.Item!;
                var manager = context.Manager;
                properties = new List<object?>(2) { (int)state, itemEntity.Id };

                var i = 3;
                var newName = context.Services.Language.GetString(item, item.GetQuantity(), SenseType.Sight);
                if (snapshot!.NameSnapshot != newName)
                {
                    properties.Add(i);
                    properties.Add(newName);
                }

                i++;
                var physicalObserver = context.Observer.Physical!;
                var physicalObserverEntry = context.DbContext.Entry(physicalObserver);
                if (physicalObserverEntry.Property(nameof(PhysicalComponent.Size)).IsModified)
                {
                    var slots = manager.ItemUsageSystem.GetEquipableSlots(item, physicalObserver.Size)
                        .GetNonRedundantFlags(removeComposites: true)
                        .Select(s => Serialize(s, context))
                        .ToList();

                    properties.Add(i);
                    properties.Add(slots);
                }

                i++;
                var equippedSlot = context.DbContext.Entry(item).Property(nameof(ItemComponent.EquippedSlot));
                if (equippedSlot.IsModified)
                {
                    properties.Add(i);
                    properties.Add(item.EquippedSlot == EquipmentSlot.None
                        ? null
                        : Serialize(item.EquippedSlot, context));
                }

                return properties.Count > 2 ? properties : null;
            }
        }
    }

    private static object[] Serialize(EquipmentSlot slot, SerializationContext context) => new object[]
    {
        (int)slot, context.Services.Language.GetString(slot, context.Observer, abbreviate: true),
        context.Services.Language.GetString(slot, context.Observer, abbreviate: false)
    };

    public static List<object> SerializeAttributes(GameEntity? itemEntity, SenseType sense, SerializationContext context)
    {
        var canIdentify = itemEntity != null && sense.CanIdentify();
        if (!canIdentify)
        {
            return new List<object>();
        }

        var manager = itemEntity!.Manager;
        var item = itemEntity.Item!;
        var template = Item.Loader.Get(item.TemplateName);
        var physical = itemEntity.Physical!;
        var equipableSlots = manager.ItemUsageSystem.GetEquipableSlots(item, context.Observer.Physical!.Size)
            .GetNonRedundantFlags(removeComposites: true)
            .Select(s => Serialize(s, context))
            .ToList();
        return new List<object>(12)
        {
            context.Services.Language.GetString(item, item.GetQuantity(), sense),
            context.Services.Language.GetDescription(item.TemplateName, DescriptionCategory.Item),
            item.Type,
            (int)physical.Material,
            physical.Weight,
            template.Complexity ?? ItemComplexity.Normal,
            template.RequiredMight ?? 0,
            template.RequiredSpeed ?? 0,
            template.RequiredFocus ?? 0,
            template.RequiredPerception ?? 0,
            equipableSlots,
            physical.Abilities
                .Where(a => a.Ability!.IsUsable
                            && a.Ability.Activation != ActivationType.Default
                            && a.Ability.Activation != ActivationType.Always
                            && (a.Ability.Name == null
                                || !a.Ability.Name.StartsWith("Add", StringComparison.Ordinal)
                                || !a.Ability.Name.EndsWith("ability", StringComparison.Ordinal)))
                .Select(a => AbilitySnapshot.SerializeAttributes(a, context.Observer, context)).ToList()
        };
    }
}
