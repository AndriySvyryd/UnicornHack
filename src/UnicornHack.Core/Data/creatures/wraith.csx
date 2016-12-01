new CreatureVariant
{
    Name = "wraith",
    Species = Species.Wraith,
    SpeciesClass = SpeciesClass.Undead,
    CorpseVariantName = "",
    InitialLevel = 6,
    ArmorClass = 4,
    MagicResistance = 15,
    MovementRate = 12,
    Size = Size.Medium,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new DrainLife { Amount = 2 } }
        }
,
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new AbilityEffect[] { new DrainLife { Amount = 1 } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new DrainLife { Amount = 1 } } }
    }
,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "Flight",
        "FlightControl",
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
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 } },
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.Stalking,
    Alignment = -6,
    Noise = ActorNoiseType.Howl
}
