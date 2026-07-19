namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item FreezingFocus = new Item
    {
        Name = "freezing focus",
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "freezing focus ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Hit,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+RequirementsModifier()",
                Delay = "100*RequirementsModifier()",
                Effects = new List<Effect> { new ColdDamage { Amount = "40*RequirementsModifier()" } }
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
