namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item Shortbow = new Item
    {
        Name = "shortbow",
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "shortbow ability #1",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.OnRangedAttack,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Shoot,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "10+RequirementsModifier()",
                Delay = "100*RequirementsModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "30*RequirementsModifier()" }, new Activate { Projectile = "arrow" } }
            }
        },
        Type = ItemType.WeaponRangedLong,
        Material = Material.Wood,
        Weight = 5,
        EquipableSizes = SizeCategory.Small | SizeCategory.Medium,
        EquipableSlots = EquipmentSlot.GraspRanged,
        RequiredMight = 5,
        RequiredPerception = 5,
        RequiredSpeed = 10
    };
}
