namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability CombatReflexes = new Ability
    {
        Name = "Combat Reflexes",
        Type = AbilityType.Feat,
        GenerationWeight = "$feature == 'feat pedestal' ? 1.0 : 0.0",
        Effects = new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "2", PropertyName = "Evasion" } }
    };
}
