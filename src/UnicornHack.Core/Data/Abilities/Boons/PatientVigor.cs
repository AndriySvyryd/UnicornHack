namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability PatientVigor = new Ability
    {
        Name = "Patient Vigor",
        Type = AbilityType.Boon,
        Effects = new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "10", PropertyName = "EnergyPointMaximum" } }
    };
}
