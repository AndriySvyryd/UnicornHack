namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability Restoration = new Ability
    {
        Name = "restoration",
        Type = AbilityType.Buff,
        GenerationWeight = "$feature == 'small fountain' ? 1.0 : 0.0",
        Activation = ActivationType.Default,
        Effects = new List<Effect> { new Recharge { Amount = "20" } }
    };
}
