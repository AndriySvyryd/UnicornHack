namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability BroadFrame = new Ability
    {
        Name = "Broad Frame",
        Type = AbilityType.Trait,
        GenerationWeight = "$feature == 'trait pedestal' ? 1.0 : 0.0",
        Effects = new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "1", PropertyName = "Might" } }
    };
}
