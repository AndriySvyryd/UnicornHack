using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item MailArmor = new Item
    {
        Name = "mail armor",
        Abilities = new HashSet<Ability>
        {
            new Ability { Activation = ActivationType.WhileEquipped, Effects = new List<Effect> { new ChangeProperty<int> { PropertyName = "Armor", Value = 5 } } }
        },
        Type = ItemType.ArmorTorso,
        Material = Material.Steel,
        Weight = 40,
        EquipableSizes = SizeCategory.Small | SizeCategory.Medium | SizeCategory.Large,
        EquipableSlots = EquipmentSlot.Torso
    };
}
