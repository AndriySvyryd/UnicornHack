namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability HiveSense = new Ability
    {
        Name = "Hive-sense",
        Type = AbilityType.Boon,
        Effects = new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "1", PropertyName = "RoomPerceptionRange" } }
    };
}
