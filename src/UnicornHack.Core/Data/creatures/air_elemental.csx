new CreatureVariant
{
    InitialLevel = 8,
    ArmorClass = 2,
    MagicResistance = 30,
    GenerationFrequency = Frequency.Uncommonly,
    CorpseVariantName = "",
    Name = "air elemental",
    Species = Species.Elemental,
    SpeciesClass = SpeciesClass.Extraplanar,
    MovementRate = 36,
    Size = Size.Huge,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "Flight",
        "FlightControl",
        "Invisibility",
        "NonAnimal",
        "NonSolidBody",
        "Breathlessness",
        "Limblessness",
        "Eyelessness",
        "Headlessness",
        "Mindlessness",
        "Asexuality"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "StoningResistance", 3 }, { "SlimingResistance", 3 }, { "SicknessResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Engulf { Duration = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnDigestion,
            Action = AbilityAction.Digestion,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Deafen { Duration = 2 } }
        }
    }
}
