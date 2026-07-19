namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability Innate = new Ability
    {
        Name = "innate",
        Type = AbilityType.Innate,
        Effects = new List<Effect>
        {
            new ChangeProperty<int> { TargetActivator = true, Amount = "100*MightModifier()-100", PropertyName = "HitPointMaximum" },
            new ChangeProperty<int> { TargetActivator = true, Amount = "100*FocusModifier()-100", PropertyName = "EnergyPointMaximum" },
            new ChangeProperty<int> { TargetActivator = true, Amount = "50*MightModifier()-50", PropertyName = "PhysicalStatusResistance" },
            new ChangeProperty<int> { TargetActivator = true, Amount = "50*FocusModifier()-50", PropertyName = "MentalStatusResistance" },
            new ChangeProperty<int> { TargetActivator = true, Amount = "10*$perception", PropertyName = "Accuracy" },
            new ChangeProperty<int> { TargetActivator = true, Amount = "50+5*($speed-$hindrance/10)", PropertyName = "Evasion" },
            new ChangeProperty<int> { TargetActivator = true, Amount = "($speed-$hindrance/10)==0?0:1000/($speed-$hindrance/10)", PropertyName = "MovementDelay" },
            new ChangeProperty<int> { TargetActivator = true, Amount = "($speed-$hindrance/10)==0?0:1000/($speed-$hindrance/10)", PropertyName = "TurningDelay" },
            new ChangeProperty<int> { TargetActivator = true, Amount = "10*PerceptionModifier()", PropertyName = "HearingLevel" },
            new ChangeProperty<int> { TargetActivator = true, Amount = "200-$hindrance", PropertyName = "Quietness" },
            new ChangeProperty<int> { TargetActivator = true, Amount = "10", PropertyName = "Capacity" }
        }
    };
}
