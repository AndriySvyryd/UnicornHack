namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability Renewal = new Ability
    {
        Name = "renewal",
        Type = AbilityType.Buff,
        GenerationWeight = "$feature == 'small fountain' ? 1.0 : 0.0",
        Activation = ActivationType.Default,
        Effects = new List<Effect> { new Heal { Amount = "20" } }
    };
}
