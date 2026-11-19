namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability LiftMajorCurse = new Ability
    {
        Name = "lift major curse",
        Type = AbilityType.Buff,
        GenerationWeight = "$feature == 'big fountain' ? 1.0 : 0.0",
        Activation = ActivationType.Default,
        Effects = new List<Effect> { new DecreaseAbilityLevel { Duration = EffectDuration.Instant, AbilityType = AbilityType.Curse } }
    };
}
