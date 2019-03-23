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
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnRangedAttack,
                    Range = 10,
                    Action = AbilityAction.Shoot,
                    Delay = "100*weaponScaling",
                    Effects = new HashSet<Effect>
                        {new Burn {Damage = "30*weaponScaling"}, new Activate {Projectile = "fire bolt"}}
                }
            },
            Type = ItemType.WeaponRangedLong,
            Material = Material.Wood,
            Weight = 5,
            EquipableSizes = SizeCategory.Medium | SizeCategory.Large,
            EquipableSlots = EquipmentSlot.GraspRanged,
            RequiredFocus = 10
        };
    }
}
