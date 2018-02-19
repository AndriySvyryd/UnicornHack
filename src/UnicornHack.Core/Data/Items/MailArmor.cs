using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Items
{
    public static partial class ItemVariantData
    {
        public static readonly ItemVariant MailArmor = new ItemVariant
        {
            Name = "mail armor",
            Type = ItemType.ArmorTorso,
            Material = Material.Steel,
            EquipableSlots = EquipmentSlot.Torso,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.WhileEquipped,
                    Effects = new HashSet<Effect>
                    {
                        new ChangeProperty<int> {PropertyName = "physical deflection", Value = 10},
                        new ChangeProperty<int> {PropertyName = "physical absorption", Value = 2}
                    }
                }
            },
            ValuedProperties = new Dictionary<string, object> {{"weight", 40}}
        };
    }
}