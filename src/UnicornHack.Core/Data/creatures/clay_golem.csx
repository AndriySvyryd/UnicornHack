new Creature
{
    Name = "clay golem",
    Species = Species.Golem,
    ArmorClass = 7,
    MagicResistance = 40,
    MovementRate = 7,
    Weight = 1500,
    Size = Size.Large,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 16 } }
        }
    }
,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "NonAnimal",
        "Breathlessness",
        "Mindlessness",
        "Humanoidness",
        "Asexuality",
        "SlimingResistance",
        "SicknessResistance"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "ElectricityResistance", 3 }, { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "ThickHide", 3 }, { "MaxHP", 50 } },
    InitialLevel = 11,
    CorpseName = "",
    GenerationFrequency = Frequency.Rarely
}
