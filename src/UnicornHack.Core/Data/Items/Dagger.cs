using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Items
{
    public static partial class ItemVariantData
    {
        public static readonly ItemVariant Dagger = new ItemVariant
        {
            Name = "dagger",
            Type = ItemType.WeaponMeleeShort,
            Weight = 5,
            Material = Material.Steel,
            EquipableSizes = Size.Tiny | Size.Small,
            EquipableSlots = EquipmentSlot.GraspSingleExtremity | EquipmentSlot.GraspBothExtremities,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = AbilityActivation.OnMeleeAttack,
                    Action = AbilityAction.Slash,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                }
            }
        };
    }
}