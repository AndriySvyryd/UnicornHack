namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability LevelKeenBargain = new Ability
    {
        Name = "level keen bargain",
        Type = AbilityType.Buff,
        GenerationWeight = "$feature == 'major shrine' ? 1.0 : 0.0",
        Activation = ActivationType.Default,
        Effects = new List<Effect> { new AddAbility { Duration = EffectDuration.UntilNextLevel, Name = "Keen Bargain" } }
    };
}
