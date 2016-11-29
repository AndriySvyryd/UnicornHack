new CreatureVariant
{
    InitialLevel = 6,
    ArmorClass = 4,
    MagicResistance = 30,
    GenerationFrequency = Frequency.Sometimes,
    Alignment = -3,
    Noise = ActorNoiseType.Moan,
    CorpseVariantName = "elf",
    Name = "elf mummy",
    Species = Species.Elf,
    SpeciesClass = SpeciesClass.Undead,
    MovementRate = 12,
    Size = Size.Medium,
    Weight = 800,
    Nutrition = 150,
    SimpleProperties = new HashSet<string> { "SleepResistance", "InvisibilityDetection", "Infravision", "Humanoidness", "Breathlessness" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 }, { "SicknessResistance", 3 } },
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
}
