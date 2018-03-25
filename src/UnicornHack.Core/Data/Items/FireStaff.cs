using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Items
{
    public static partial class ItemData
    {
        public static readonly Item FireStaff = new Item
        {
            Name = "fire staff",
            Type = ItemType.WeaponMagicStaff,
            Material = Material.Wood,
            Weight = 5,
            EquipableSizes = SizeCategory.Medium | SizeCategory.Large,
            EquipableSlots = EquipmentSlot.GraspSingleExtremity | EquipmentSlot.GraspBothExtremities,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnPhysicalRangedAttack,
                    Action = AbilityAction.Shoot,
                    Effects = new HashSet<Effect>
                    {
                        new Burn {Damage = 30},
                        new Activate {Projectile = "fire bolt"}
                    }
                }
            }
        };
    }
}
