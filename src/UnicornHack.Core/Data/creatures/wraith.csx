new CreatureVariant
{
    InitialLevel = 6,
    ArmorClass = 4,
    MagicResistance = 15,
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.Stalking,
    Alignment = -6,
    Noise = ActorNoiseType.Howl,
    CorpseVariantName = "",
    Name = "wraith",
    Species = Species.Wraith,
    SpeciesClass = SpeciesClass.Undead,
    MovementRate = 12,
    Size = Size.Medium,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Flight", "FlightControl", "Infravision", "NonSolidBody", "Humanoidness", "Breathlessness", "NoInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 }, { "SicknessResistance", 3 }, { "StoningResistance", 3 }, { "SlimingResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new DrainLife { Amount = 2 } }
        }
,
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new AbilityEffect[] { new DrainLife { Amount = 1 } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new DrainLife { Amount = 1 } } }
    }
}
