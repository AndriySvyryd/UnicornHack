using System.Collections.Generic;
using UnicornHack.Generation;

namespace UnicornHack.Data.Items
{
    public static partial class ItemVariantData
    {
        public static readonly ItemVariant MailArmor = new ItemVariant
        {
            Name = "mail armor",
            Type = ItemType.ArmorBody,
            Material = Material.Steel,
            EquipableSlots = EquipmentSlot.Body,
            ValuedProperties =
                new Dictionary<string, object> {{"physical deflection", 10}, {"physical absorption", 2}, {"weight", 40}}
        };
    }
}