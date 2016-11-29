new CreatureVariant
{
    InitialLevel = 12,
    ArmorClass = 10,
    MagicResistance = 25,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.Wandering | MonsterBehavior.Stalking,
    Alignment = -6,
    Noise = ActorNoiseType.Howl,
    CorpseVariantName = "",
    PreviousStageName = "ghost",
    Name = "shade",
    Species = Species.Ghost,
    SpeciesClass = SpeciesClass.Undead,
    MovementRate = 10,
    Size = Size.Medium,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "Flight",
        "FlightControl",
        "Phasing",
        "Infravision",
        "InvisibilityDetection",
        "NonSolidBody",
        "Humanoidness",
        "Breathlessness",
        "NoInventory"
    }
,
    ValuedProperties = new Dictionary<string, Object>
    {
        {
            "ColdResistance",
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
            "SicknessResistance",
            3
        },
        {
            "StoningResistance",
            3
        },
        {
            "SlimingResistance",
            3
        }
    }
,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Paralyze { Duration = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Slow { Duration = 3 } }
        }
    }
}
