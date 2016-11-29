new CreatureVariant
{
    InitialLevel = 10,
    ArmorClass = -5,
    MagicResistance = 15,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.Wandering | MonsterBehavior.Stalking,
    Alignment = -6,
    CorpseVariantName = "",
    NextStageName = "shade",
    Name = "ghost",
    Species = Species.Ghost,
    SpeciesClass = SpeciesClass.Undead,
    MovementRate = 3,
    Size = Size.Medium,
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
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
    }
}
