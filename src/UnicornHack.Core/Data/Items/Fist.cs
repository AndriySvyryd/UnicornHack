namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item Fist = new Item
    {
        Name = "fist",
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "fist ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Punch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "10+RequirementsModifier()",
                Delay = "100*RequirementsModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "20*RequirementsModifier()" } }
            }
        },
        Type = ItemType.WeaponMeleeHand,
        GenerationWeight = "0",
        Material = Material.Flesh,
        Weight = 4,
        RequiredMight = 2,
        RequiredSpeed = 2
    };
}
