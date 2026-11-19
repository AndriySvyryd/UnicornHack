namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability BranchSteadfast = new Ability
    {
        Name = "branch steadfast",
        Type = AbilityType.Buff,
        GenerationWeight = "$feature == 'deity altar' ? 1.0 : 0.0",
        Activation = ActivationType.Default,
        Effects = new List<Effect> { new AddAbility { Duration = EffectDuration.UntilNextBranch, Name = "Steadfast" } }
    };
}
