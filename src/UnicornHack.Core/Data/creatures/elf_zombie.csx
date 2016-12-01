new CreatureVariant
{
    Name = "elf zombie",
    Species = Species.Elf,
    SpeciesClass = SpeciesClass.Undead,
    CorpseVariantName = "elf",
    InitialLevel = 4,
    ArmorClass = 9,
    MagicResistance = 10,
    MovementRate = 12,
    Weight = 800,
    Size = Size.Medium,
    Nutrition = 150,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Infect { } } }
    }
,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "InvisibilityDetection",
        "Infravision",
        "Humanoidness",
        "Breathlessness",
        "Mindlessness",
        "SicknessResistance"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 } },
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.Stalking,
    Noise = ActorNoiseType.Moan
}
