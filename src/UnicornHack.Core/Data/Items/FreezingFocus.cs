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
                    Effects = new HashSet<Effect> {new Freeze {Damage = 40}}
                }
            },
            Type = ItemType.WeaponMeleeShort,
            Material = Material.Bone,
            Weight = 5,
            EquipableSizes = SizeCategory.Small | SizeCategory.Medium,
            EquipableSlots = EquipmentSlot.GraspMelee
        };
    }
}
