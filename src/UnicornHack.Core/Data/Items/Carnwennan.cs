namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item Carnwennan = new Item
    {
        Name = "Carnwennan",
        BaseName = "dagger",
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "Carnwennan edge",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Slash,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+RequirementsModifier()",
                Delay = "100*RequirementsModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "40*RequirementsModifier()" } }
            },
            new Ability
            {
                Name = "Orddu's Opening",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Stab,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+RequirementsModifier()",
                XPCooldown = 300,
                Delay = "100*RequirementsModifier()",
                Effects = new List<Effect>
                {
                    new PhysicalDamage { Amount = "40*RequirementsModifier()", ArmorPenetration = "3" },
                    new Stagger(),
                    new DecreaseAbilityLevel { Duration = EffectDuration.UntilTimeout, DurationAmount = "100", AbilityName = "warded" }
                }
            }
        },
        Complexity = ItemComplexity.Intricate,
        GenerationWeight = "$branch == 'ruined stronghold' && $depth >= 2 ? 0.15 : 0"
    };
}
