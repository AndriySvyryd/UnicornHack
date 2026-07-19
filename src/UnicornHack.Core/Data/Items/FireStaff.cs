namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item FireStaff = new Item
    {
        Name = "fire staff",
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "fire staff ability #1",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.OnRangedAttack,
                Trigger = ActivationType.OnRangedAttack,
                Range = 10,
                Action = AbilityAction.Shoot,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+RequirementsModifier()",
                Delay = "100*RequirementsModifier()",
                Effects = new List<Effect>
                {
                    new FireDamage { Amount = "30*RequirementsModifier()" },
                    new FireDamage { Duration = EffectDuration.UntilTimeout, Amount = "30*RequirementsModifier()" },
                    new Activate { Projectile = "fire bolt" }
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
