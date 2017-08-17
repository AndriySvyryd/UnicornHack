using System;
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
            Weight = 40,
            Material = Material.Steel,
            EquipableSlots = EquipmentSlot.Body,
            ValuedProperties = new Dictionary<string, Object> {{"ArmorClass", 10}}
        };
    }
}