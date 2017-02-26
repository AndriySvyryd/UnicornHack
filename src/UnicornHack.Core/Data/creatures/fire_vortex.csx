new Creature
{
    Name = "fire vortex",
    Species = Species.Vortex,
    SpeciesClass = SpeciesClass.Extraplanar,
    ArmorClass = 2,
    MagicResistance = 30,
    MovementDelay = 54,
    Size = Size.Huge,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new Engulf { Duration = 4 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnDigestion,
            Action = AbilityAction.Digestion,
            Timeout = 1,
            Effects = new HashSet<Effect> { new FireDamage { Damage = 5 } }
        }
,
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new HashSet<Effect> { new FireDamage { Damage = 5 } } },
        new Ability { Activation = AbilityActivation.OnRangedHit, Effects = new HashSet<Effect> { new FireDamage { Damage = 5 } } }
    }
,
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
        "Asexuality",
        "StoningResistance",
        "SlimingResistance",
        "SicknessResistance"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 }, { "AcidResistance", 3 }, { "PoisonResistance", 3 }, { "VenomResistance", 3 } },
    InitialLevel = 8,
    CorpseName = "",
    GenerationFrequency = Frequency.Commonly
}
