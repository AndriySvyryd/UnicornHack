new CreatureVariant
{
    InitialLevel = 30,
    ArmorClass = -5,
    MagicResistance = 100,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable | GenerationFlags.HellOnly,
    Behavior = MonsterBehavior.Stalking | MonsterBehavior.Displacing,
    Noise = ActorNoiseType.Rider,
    Name = "Pestilence",
    Species = Species.Horseman,
    SpeciesClass = SpeciesClass.Extraplanar,
    MovementRate = 12,
    Size = Size.Medium,
    Weight = 1000,
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
        "Maleness"
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
        },
        {
            "Regeneration",
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
            Effects = new AbilityEffect[] { new ScriptedEffect { Script = "Pestilence" } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new ScriptedEffect { Script = "Pestilence" } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new ScriptedEffect { Script = "Pestilence" } } }
    }
}
