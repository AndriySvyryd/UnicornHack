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
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnRangedAttack,
                    Range = 15,
                    Action = AbilityAction.Throw,
                    Delay = "100*weaponScaling",
                    Effects = new HashSet<Effect>
                        {new PhysicalDamage {Damage = "30*weaponScaling"}, new Activate {Projectile = "throwing knife"}}
                }
            },
            Type = ItemType.WeaponRangedClose,
            Material = Material.Steel,
            Weight = 5,
            EquipableSizes = SizeCategory.Small | SizeCategory.Medium,
            EquipableSlots = EquipmentSlot.GraspSingleRanged,
            RequiredMight = 5,
            RequiredSpeed = 5
        };
    }
}
