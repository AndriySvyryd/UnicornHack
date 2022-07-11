namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability IceShard = new Ability
    {
        Name = "ice shard",
        Activation = ActivationType.Targeted,
        Trigger = ActivationType.OnRangedAttack,
        Range = 10,
        Action = AbilityAction.Shoot,
        SuccessCondition = AbilitySuccessCondition.NormalAttack,
        Accuracy = "5+PerceptionModifier()",
        Cooldown = 1000,
        Delay = "100",
        Effects = new List<Effect> { new Freeze { Damage = "40" } }
    };
}
