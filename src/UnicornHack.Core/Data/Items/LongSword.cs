using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Items
{
    public static partial class ItemVariantData
    {
        public static readonly ItemVariant LongSword = new ItemVariant
        {
            Name = "long sword",
            Type = ItemType.WeaponMeleeMedium,
            Material = Material.Steel,
            EquipableSizes = SizeCategory.Medium | SizeCategory.Large,
            EquipableSlots = EquipmentSlot.GraspSingleExtremity | EquipmentSlot.GraspBothExtremities,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnMeleeAttack,
                    Action = AbilityAction.Slash,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 20}}
                }
            },
            ValuedProperties = new Dictionary<string, object> {{"weight", 10}}
        };
    }
}