using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Items
{
    public static partial class ItemData
    {
        public static readonly Item FreezingFocus = new Item
        {
            Name = "freezing focus",
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnMeleeAttack,
                    Range = 1,
                    Action = AbilityAction.Hit,
                    Delay = "100*weaponScaling",
                    Effects = new HashSet<Effect> {new Freeze {Damage = "40*weaponScaling"}}
                }
            },
            Type = ItemType.WeaponMeleeShort,
            Material = Material.Bone,
            Weight = 5,
            EquipableSizes = SizeCategory.Small | SizeCategory.Medium,
            EquipableSlots = EquipmentSlot.GraspMelee,
            RequiredFocus = 8,
            RequiredSpeed = 2
        };
    }
}
