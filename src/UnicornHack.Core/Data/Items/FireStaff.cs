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
                    Trigger = ActivationType.OnRangedAttack,
                    Range = 10,
                    Action = AbilityAction.Shoot,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+RequirementsModifier()",
                    Delay = "100*RequirementsModifier()",
                    Effects = new List<Effect>
                    {
                        new Burn {Damage = "30*RequirementsModifier()"},
                        new Activate {Projectile = "fire bolt"}
                    }
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
