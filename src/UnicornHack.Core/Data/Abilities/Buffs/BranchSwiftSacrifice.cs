namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability BranchSwiftSacrifice = new Ability
    {
        Name = "branch swift sacrifice",
        Type = AbilityType.Buff,
        GenerationWeight = "$feature == 'deity altar' ? 1.0 : 0.0",
        Activation = ActivationType.Default,
        Effects = new List<Effect> { new AddAbility { Duration = EffectDuration.UntilNextBranch, Name = "Swift Sacrifice" } }
    };
}
