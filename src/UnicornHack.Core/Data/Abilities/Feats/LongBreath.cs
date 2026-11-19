namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability LongBreath = new Ability
    {
        Name = "Long Breath",
        Type = AbilityType.Feat,
        GenerationWeight = "$feature == 'feat pedestal' ? 1.0 : 0.0",
        Effects = new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "5", PropertyName = "HitPointMaximum" } }
    };
}
