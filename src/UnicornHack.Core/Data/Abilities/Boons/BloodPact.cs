namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability BloodPact = new Ability
    {
        Name = "Blood Pact",
        Type = AbilityType.Boon,
        GenerationWeight = "$feature == 'ritual shrine' ? 1.0 : 0.0",
        Effects = new List<Effect>
        {
            new ChangeProperty<int> { TargetActivator = true, Amount = "10", PropertyName = "HitPointMaximum" },
            new ChangeProperty<int> { TargetActivator = true, Amount = "-1", PropertyName = "Focus" }
        }
    };
}
