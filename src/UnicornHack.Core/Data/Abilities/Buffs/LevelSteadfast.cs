namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability LevelSteadfast = new Ability
    {
        Name = "level steadfast",
        Type = AbilityType.Buff,
        GenerationWeight = "$feature == 'major shrine' ? 1.0 : 0.0",
        Activation = ActivationType.Default,
        Effects = new List<Effect> { new AddAbility { Duration = EffectDuration.UntilNextLevel, Name = "Steadfast" } }
    };
}
