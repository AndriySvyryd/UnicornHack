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
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnRangedAttack,
                    Range = 20,
                    Action = AbilityAction.Shoot,
                    Delay = "100*weaponScaling",
                    Effects = new HashSet<Effect>
                        {new PhysicalDamage {Damage = "30*weaponScaling"}, new Activate {Projectile = "arrow"}}
                }
            },
            Type = ItemType.WeaponRangedLong,
            Material = Material.Wood,
            Weight = 5,
            EquipableSizes = SizeCategory.Small | SizeCategory.Medium,
            EquipableSlots = EquipmentSlot.GraspRanged,
            RequiredMight = 5,
            RequiredSpeed = 10
        };
    }
}
