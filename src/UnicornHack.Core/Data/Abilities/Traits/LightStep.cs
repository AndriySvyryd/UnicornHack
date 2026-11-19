namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability LightStep = new Ability
    {
        Name = "Light Step",
        Type = AbilityType.Trait,
        GenerationWeight = "$feature == 'trait pedestal' ? 1.0 : 0.0",
        Effects = new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "1", PropertyName = "Speed" } }
    };
}
