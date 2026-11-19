namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability BrightMind = new Ability
    {
        Name = "Bright Mind",
        Type = AbilityType.Trait,
        GenerationWeight = "$feature == 'trait pedestal' ? 1.0 : 0.0",
        Effects = new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "1", PropertyName = "Focus" } }
    };
}
