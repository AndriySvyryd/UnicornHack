namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability Insight = new Ability
    {
        Name = "insight",
        Type = AbilityType.Buff,
        GenerationWeight = "$feature == 'small fountain' || $feature == 'big fountain' ? 1.0 : 0.0",
        Activation = ActivationType.Default,
        Effects = new List<Effect> { new GainXP { Amount = "20" } }
    };
}
