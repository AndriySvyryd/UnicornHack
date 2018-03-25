using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Items
{
    public static partial class ItemData
    {
        public static readonly Item ThrowingKnives = new Item
        {
            Name = "throwing knives",
            Type = ItemType.WeaponRangedThrown,
            Material = Material.Steel,
            Weight = 5,
            EquipableSizes = SizeCategory.Small | SizeCategory.Medium,
            EquipableSlots = EquipmentSlot.GraspSingleExtremity,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnPhysicalRangedAttack,
                    Action = AbilityAction.Throw,
                    Effects = new HashSet<Effect>
                    {
                        new PhysicalDamage {Damage = 30},
                        new Activate {Projectile = "throwing knife"}
                    }
                }
            }
        };
    }
}
