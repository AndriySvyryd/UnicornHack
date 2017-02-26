new Creature
{
    Name = "ghost",
    Species = Species.Ghost,
    SpeciesClass = SpeciesClass.Undead,
    ArmorClass = -5,
    MagicResistance = 15,
    MovementDelay = 400,
    Size = Size.Medium,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 1 } }
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
    InitialLevel = 10,
    NextStageName = "shade",
    CorpseName = "",
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.Wandering | MonsterBehavior.Stalking,
    Alignment = -6
}
