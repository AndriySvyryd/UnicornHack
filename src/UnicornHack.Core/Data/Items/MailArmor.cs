using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Items
{
    public static partial class ItemData
    {
        public static readonly Item MailArmor = new Item
        {
            Name = "mail armor",
            Type = ItemType.ArmorTorso,
            Material = Material.Steel,
            Weight = 40,
            EquipableSizes = SizeCategory.Small | SizeCategory.Medium | SizeCategory.Large,
            EquipableSlots = EquipmentSlot.Torso,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.WhileEquipped,
                    Effects = new HashSet<Effect>
                    {
                        new ChangeProperty<int>
                        {
                            PropertyName = "physicalDeflection",
                            Value = 10
                        },
                        new ChangeProperty<int>
                        {
                            PropertyName = "physicalAbsorption",
                            Value = 2
                        }
                    }
                }
            }
        };
    }
}
