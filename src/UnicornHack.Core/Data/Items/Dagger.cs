using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item Dagger = new Item
    {
        Name = "dagger",
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Activation = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Slash,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+RequirementsModifier()",
                Delay = "100*RequirementsModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Damage = "40*RequirementsModifier()" } }
            }
        },
        Type = ItemType.WeaponMeleeShort,
        Material = Material.Steel,
        Weight = 5,
        EquipableSizes = SizeCategory.Tiny | SizeCategory.Small,
        EquipableSlots = EquipmentSlot.GraspMelee,
        RequiredMight = 5,
        RequiredSpeed = 10
    };
}
