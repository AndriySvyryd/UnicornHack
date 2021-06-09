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
                    Trigger = ActivationType.OnRangedAttack,
                    Range = 15,
                    Action = AbilityAction.Throw,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+RequirementsModifier()",
                    Delay = "100*RequirementsModifier()",
                    Effects = new List<Effect>
                    {
                        new PhysicalDamage {Damage = "30*RequirementsModifier()"},
                        new Activate {Projectile = "throwing knife"}
                    }
                }
            },
            Type = ItemType.WeaponRangedClose,
            Material = Material.Steel,
            Weight = 5,
            EquipableSizes = SizeCategory.Small | SizeCategory.Medium,
            EquipableSlots = EquipmentSlot.GraspSingleRanged,
            RequiredMight = 5,
            RequiredPerception = 2,
            RequiredSpeed = 5
        };
    }
}
