namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability Enervated = new Ability
    {
        Name = "enervated",
        Type = AbilityType.Status,
        Effects = new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "<propagated>", PropertyName = "ReservedEnergyPoints" } }
    };
}
