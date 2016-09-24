new CreatureVariant
{
    InitialLevel = 6,
    ArmorClass = 2,
    MagicResistance = 30,
    GenerationFrequency = Frequency.Commonly,
    CorpseVariantName = "",
    Name = "energy vortex",
    Species = Species.Vortex,
    SpeciesClass = SpeciesClass.Extraplanar,
    MovementRate = 20,
    Size = Size.Huge,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "Flight",
        "FlightControl",
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
    ValuedProperties = new Dictionary<string, Object>
    {
        {
            "ElectricityResistance",
            3
        },
        {
            "DisintegrationResistance",
            3
        },
        {
            "PoisonResistance",
            3
        },
        {
            "VenomResistance",
            3
        },
        {
            "StoningResistance",
            3
        },
        {
            "SlimingResistance",
            3
        },
        {
            "SicknessResistance",
            3
        }
    }
,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Engulf { Duration = 4 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnDigestion,
            Action = AbilityAction.Digestion,
            Timeout = 1,
            Effects = new AbilityEffect[] { new ElectricityDamage { Damage = 3 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnDigestion,
            Action = AbilityAction.Digestion,
            Timeout = 1,
            Effects = new AbilityEffect[] { new DrainEnergy { Amount = 3 } }
        }
,
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new AbilityEffect[] { new ElectricityDamage { Damage = 3 } } },
        new Ability { Activation = AbilityActivation.OnRangedHit, Effects = new AbilityEffect[] { new ElectricityDamage { Damage = 3 } } }
    }
}
