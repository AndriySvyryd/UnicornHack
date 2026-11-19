namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability BranchBloodPact = new Ability
    {
        Name = "branch blood pact",
        Type = AbilityType.Buff,
        GenerationWeight = "$feature == 'deity altar' ? 1.0 : 0.0",
        Activation = ActivationType.Default,
        Effects = new List<Effect> { new AddAbility { Duration = EffectDuration.UntilNextBranch, Name = "Blood Pact" } }
    };
}
