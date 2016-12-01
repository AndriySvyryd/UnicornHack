new CreatureVariant
{
    Name = "elf mummy",
    Species = Species.Elf,
    SpeciesClass = SpeciesClass.Undead,
    CorpseVariantName = "elf",
    InitialLevel = 6,
    ArmorClass = 4,
    MagicResistance = 30,
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
    SimpleProperties = new HashSet<string> { "SleepResistance", "InvisibilityDetection", "Infravision", "Humanoidness", "Breathlessness", "SicknessResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 } },
    GenerationFrequency = Frequency.Sometimes,
    Alignment = -3,
    Noise = ActorNoiseType.Moan
}
