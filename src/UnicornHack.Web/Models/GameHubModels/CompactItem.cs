using System.Linq;
using UnicornHack.Data;
using UnicornHack.Services;
using UnicornHack.Utils;

// ReSharper disable RedundantAssignment
namespace UnicornHack.Models.GameHubModels
{
    public class CompactItem
    {
        public object[] Properties { get; set; }
        private static readonly int ItemPropertyCount = 8;

        public static CompactItem Serialize(Item item, GameDbContext context, GameServices services)
        {
            switch (item)
            {
                case Gold gold:
                    return Serialize(gold, new object[ItemPropertyCount + 1], context, services);
                case Container container:
                    return Serialize(container, new object[ItemPropertyCount + 1], context, services);
                default:
                    return Serialize(item, new object[ItemPropertyCount], context, services);
            }
        }

        private static CompactItem Serialize(Item item, object[] properties, GameDbContext context, GameServices services)
        {
            var i = 0;
            properties[i++] = item.Id;
            properties[i++] = item.BaseName;
            properties[i++] = services.Language.ToString(item);
            properties[i++] = item.LevelX;
            properties[i++] = item.LevelY;
            properties[i++] = (int)item.Type;
            properties[i++] = item.EquippedSlot == null
                ? null
                : services.Language.ToString(item.EquippedSlot.Value, item.Actor, abbreviate: true);

            // TODO: Get correct size
            var slots = item.GetEquipableSlots(SizeCategory.Medium).GetNonRedundantFlags(removeComposites: true)
                .Select(s => new object[] {(int)s, services.Language.ToString(s, item.Actor, abbreviate: true)})
                .ToList();
            properties[i++] = slots.Any() ? slots : null;

            return new CompactItem
            {
                Properties = properties
            };
        }

        private static CompactItem Serialize(Gold gold, object[] properties, GameDbContext context, GameServices services)
        {
            var i = ItemPropertyCount;
            properties[i++] = gold.Quantity;

            return Serialize((Item)gold, properties, context, services);
        }

        private static CompactItem Serialize(Container container, object[] properties, GameDbContext context, GameServices services)
        {
            var i = ItemPropertyCount;
            properties[i++] = container.Quantity;

            return Serialize((Item)container, properties, context, services);
        }
    }
}