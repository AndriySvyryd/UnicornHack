namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability FocusedOffering = new Ability
    {
        Name = "Focused Offering",
        Type = AbilityType.Boon,
        GenerationWeight = "$feature == 'ritual shrine' ? 1.0 : 0.0",
        Effects = new List<Effect>
        {
            new ChangeProperty<int> { TargetActivator = true, Amount = "2", PropertyName = "Focus" },
            new ChangeProperty<int> { TargetActivator = true, Amount = "-5", PropertyName = "HitPointMaximum" }
        }
    };
}
