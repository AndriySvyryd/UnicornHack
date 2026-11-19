namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability BriefVigor = new Ability
    {
        Name = "brief vigor",
        Type = AbilityType.Buff,
        GenerationWeight = "0",
        Activation = ActivationType.Default,
        Effects = new List<Effect> { new AddAbility { Duration = EffectDuration.UntilTimeout, DurationAmount = "300", Name = "Patient Vigor" } }
    };
}
