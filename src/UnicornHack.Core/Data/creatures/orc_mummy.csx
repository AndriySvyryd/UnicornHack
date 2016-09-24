new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = 5,
    MagicResistance = 20,
    GenerationFrequency = Frequency.Uncommonly,
    Alignment = -4,
    Noise = ActorNoiseType.Moan,
    CorpseVariantName = "orc",
    Name = "orc mummy",
    Species = Species.Orc,
    SpeciesClass = SpeciesClass.Undead,
    MovementRate = 9,
    Size = Size.Medium,
    Weight = 1000,
    Nutrition = 100,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Humanoidness", "Breathlessness" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 }, { "SicknessResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 4 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Infect { } } }
    }
}
