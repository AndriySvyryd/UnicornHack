using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Items;

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
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "-5+RequirementsModifier()",
                Delay = "100*RequirementsModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Damage = "70*RequirementsModifier()" } }
            },
            new Ability
            {
                Activation = ActivationType.WhileEquipped,
                Effects = new List<Effect> { new ChangeProperty<int> { PropertyName = "Hindrance", Value = 1 } }
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
