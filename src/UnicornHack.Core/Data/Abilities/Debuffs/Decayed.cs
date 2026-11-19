namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability Decayed = new Ability
    {
        Name = "decayed",
        Type = AbilityType.Debuff,
        Effects = new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "<propagated>", PropertyName = "ReservedHitPoints" } }
    };
}
