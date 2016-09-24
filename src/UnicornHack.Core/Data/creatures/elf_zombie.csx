new CreatureVariant
{
    InitialLevel = 4,
    ArmorClass = 9,
    MagicResistance = 10,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.Stalking,
    Noise = ActorNoiseType.Moan,
    CorpseVariantName = "elf",
    Name = "elf zombie",
    Species = Species.Elf,
    SpeciesClass = SpeciesClass.Undead,
    MovementRate = 12,
    Size = Size.Medium,
    Weight = 800,
    Nutrition = 150,
    SimpleProperties = new HashSet<string> { "SleepResistance", "InvisibilityDetection", "Infravision", "Humanoidness", "Breathlessness", "Mindlessness" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 }, { "SicknessResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Infect { } } }
    }
}
