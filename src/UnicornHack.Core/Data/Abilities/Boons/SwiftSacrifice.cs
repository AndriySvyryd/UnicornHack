namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability SwiftSacrifice = new Ability
    {
        Name = "Swift Sacrifice",
        Type = AbilityType.Boon,
        GenerationWeight = "$feature == 'ritual shrine' ? 1.0 : 0.0",
        Effects = new List<Effect>
        {
            new ChangeProperty<int> { TargetActivator = true, Amount = "2", PropertyName = "Speed" },
            new ChangeProperty<int> { TargetActivator = true, Amount = "-1", PropertyName = "Focus" }
        }
    };
}
