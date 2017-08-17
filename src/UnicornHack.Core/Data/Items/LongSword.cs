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
            Weight = 10,
            Material = Material.Steel,
            EquipableSizes = Size.Medium | Size.Large,
            EquipableSlots = EquipmentSlot.GraspSingleExtremity | EquipmentSlot.GraspBothExtremities,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = AbilityActivation.OnMeleeAttack,
                    Action = AbilityAction.Slash,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 20}}
                }
            }
        };
    }
}