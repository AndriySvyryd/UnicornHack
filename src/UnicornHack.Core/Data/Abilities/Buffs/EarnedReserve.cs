namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability EarnedReserve = new Ability
    {
        Name = "earned reserve",
        Type = AbilityType.Buff,
        GenerationWeight = "0",
        Activation = ActivationType.Default,
        Effects = new List<Effect> { new AddAbility { Duration = EffectDuration.UntilXPGained, DurationAmount = "100", Name = "Furrow's Reserve" } }
    };
}
