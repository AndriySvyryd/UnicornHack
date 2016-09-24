new CreatureVariant
{
    InitialLevel = 8,
    ArmorClass = 2,
    MagicResistance = 30,
    GenerationFrequency = Frequency.Commonly,
    CorpseVariantName = "",
    Name = "fire vortex",
    Species = Species.Vortex,
    SpeciesClass = SpeciesClass.Extraplanar,
    MovementRate = 22,
    Size = Size.Huge,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "Flight",
        "FlightControl",
        "Infravisibility",
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
            "FireResistance",
            3
        },
        {
            "AcidResistance",
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
            Effects = new AbilityEffect[] { new FireDamage { Damage = 5 } }
        }
,
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new AbilityEffect[] { new FireDamage { Damage = 5 } } },
        new Ability { Activation = AbilityActivation.OnRangedHit, Effects = new AbilityEffect[] { new FireDamage { Damage = 5 } } }
    }
}
