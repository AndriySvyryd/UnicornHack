namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability GreaterRenewal = new Ability
    {
        Name = "greater renewal",
        Type = AbilityType.Buff,
        GenerationWeight = "$feature == 'big fountain' ? 1.0 : 0.0",
        Activation = ActivationType.Default,
        Effects = new List<Effect> { new Heal { Amount = "60" } }
    };
}
