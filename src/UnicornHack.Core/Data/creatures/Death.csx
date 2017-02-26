new Creature
{
    Name = "Death",
    Species = Species.Horseman,
    SpeciesClass = SpeciesClass.Extraplanar,
    ArmorClass = -5,
    MagicResistance = 100,
    MovementDelay = 100,
    Weight = 1000,
    Size = Size.Medium,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new ScriptedEffect { Script = "Death" } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new ScriptedEffect { Script = "Death" } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new ScriptedEffect { Script = "Death" } } }
    }
,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "DecayResistance",
        "Breathlessness",
        "Reanimation",
        "Flight",
        "FlightControl",
        "TeleportationControl",
        "PolymorphControl",
        "Infravisibility",
        "Infravision",
        "InvisibilityDetection",
        "Humanoidness",
        "Maleness",
        "StoningResistance",
        "SlimingResistance",
        "SicknessResistance"
    }
,
    ValuedProperties = new Dictionary<string, Object>
    {
        {
            "AcidResistance",
            3
        },
        {
            "FireResistance",
            3
        },
        {
            "ColdResistance",
            3
        },
        {
            "ElectricityResistance",
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
            "Regeneration",
            3
        }
    }
,
    InitialLevel = 30,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable | GenerationFlags.HellOnly,
    Behavior = MonsterBehavior.Stalking | MonsterBehavior.Displacing,
    Noise = ActorNoiseType.Rider
}
