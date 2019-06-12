using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Items
{
    public static partial class ItemData
    {
        public static readonly Item LongSword = new Item
        {
            Name = "long sword",
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnMeleeAttack,
                    Range = 1,
                    Action = AbilityAction.Slash,
                    Delay = "100*weaponScaling",
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "70*weaponScaling"}}
                }
            },
            Type = ItemType.WeaponMeleeMedium,
            Material = Material.Steel,
            Weight = 10,
            EquipableSizes = SizeCategory.Medium | SizeCategory.Large,
            EquipableSlots = EquipmentSlot.GraspMelee,
            RequiredMight = 10
        };
    }
}
