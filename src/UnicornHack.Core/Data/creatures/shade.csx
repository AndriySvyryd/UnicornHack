new Creature
{
    Name = "shade",
    Species = Species.Ghost,
    SpeciesClass = SpeciesClass.Undead,
    ArmorClass = 10,
    MagicResistance = 25,
    MovementRate = 10,
    Size = Size.Medium,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new Paralyze { Duration = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new Slow { Duration = 3 } }
        }
    }
,
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
        "NoInventory",
        "StoningResistance",
        "SlimingResistance",
        "SicknessResistance"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "DisintegrationResistance", 3 }, { "PoisonResistance", 3 } },
    InitialLevel = 12,
    PreviousStageName = "ghost",
    CorpseName = "",
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.Wandering | MonsterBehavior.Stalking,
    Alignment = -6,
    Noise = ActorNoiseType.Howl
}
