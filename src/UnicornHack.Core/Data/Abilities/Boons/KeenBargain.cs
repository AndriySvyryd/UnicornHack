namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability KeenBargain = new Ability
    {
        Name = "Keen Bargain",
        Type = AbilityType.Boon,
        GenerationWeight = "$feature == 'ritual shrine' ? 1.0 : 0.0",
        Effects = new List<Effect>
        {
            new ChangeProperty<int> { TargetActivator = true, Amount = "2", PropertyName = "Perception" },
            new ChangeProperty<int> { TargetActivator = true, Amount = "-1", PropertyName = "Might" }
        }
    };
}
