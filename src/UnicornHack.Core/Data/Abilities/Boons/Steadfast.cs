namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability Steadfast = new Ability
    {
        Name = "Steadfast",
        Type = AbilityType.Boon,
        GenerationWeight = "$feature == 'ritual shrine' ? 0.0 : 1.0",
        Effects = new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "1", PropertyName = "Might" } }
    };
}
