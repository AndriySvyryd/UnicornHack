namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability FurrowsReserve = new Ability
    {
        Name = "Furrow's Reserve",
        Type = AbilityType.Boon,
        Effects = new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "10", PropertyName = "HitPointMaximum" } }
    };
}
