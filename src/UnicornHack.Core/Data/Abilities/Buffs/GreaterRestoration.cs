namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability GreaterRestoration = new Ability
    {
        Name = "greater restoration",
        Type = AbilityType.Buff,
        GenerationWeight = "$feature == 'big fountain' ? 1.0 : 0.0",
        Activation = ActivationType.Default,
        Effects = new List<Effect> { new Recharge { Amount = "60" } }
    };
}
