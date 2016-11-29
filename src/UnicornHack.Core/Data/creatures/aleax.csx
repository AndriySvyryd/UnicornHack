new CreatureVariant
{
    InitialLevel = 10,
    MagicResistance = 30,
    GenerationFlags = GenerationFlags.NoHell,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector,
    Alignment = 7,
    Noise = ActorNoiseType.Speach,
    CorpseVariantName = "",
    NextStageName = "angel",
    Name = "aleax",
    Species = Species.Angel,
    SpeciesClass = SpeciesClass.Celestial,
    MovementRate = 8,
    Size = Size.Medium,
    Weight = 1000,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Flight", "FlightControl", "Infravisibility", "Infravision", "InvisibilityDetection", "Humanoidness" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "ElectricityResistance", 3 }, { "PoisonResistance", 3 }, { "VenomResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
    }
}
