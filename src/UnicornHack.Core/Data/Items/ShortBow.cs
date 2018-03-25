using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Items
{
    public static partial class ItemData
    {
        public static readonly Item Shortbow = new Item
        {
            Name = "shortbow",
            Type = ItemType.WeaponRangedBow,
            Material = Material.Wood,
            Weight = 5,
            EquipableSizes = SizeCategory.Small | SizeCategory.Medium,
            EquipableSlots = EquipmentSlot.GraspSingleExtremity | EquipmentSlot.GraspBothExtremities,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnPhysicalRangedAttack,
                    Action = AbilityAction.Shoot,
                    Effects = new HashSet<Effect>
                    {
                        new PhysicalDamage {Damage = 30},
                        new Activate {Projectile = "arrow"}
                    }
                }
            }
        };
    }
}
