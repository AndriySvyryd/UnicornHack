namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item LongSword = new Item
    {
        Name = "long sword",
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "long sword ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Slash,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "-5+RequirementsModifier()",
                Delay = "100*RequirementsModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "70*RequirementsModifier()" } }
            },
            new Ability
            {
                Name = "long sword ability #2",
                Activation = ActivationType.WhileEquipped,
                Effects = new List<Effect> { new ChangeProperty<int> { Amount = "1", PropertyName = "Hindrance" } }
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
