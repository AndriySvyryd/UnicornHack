new Creature
{
    Name = "elf zombie",
    Species = Species.Elf,
    SpeciesClass = SpeciesClass.Undead,
    ArmorClass = 9,
    MagicResistance = 10,
    MovementDelay = 100,
    Weight = 800,
    Size = Size.Medium,
    Nutrition = 150,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new Infect { } } }
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
    InitialLevel = 4,
    CorpseName = "elf",
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.Stalking,
    Noise = ActorNoiseType.Moan
}
