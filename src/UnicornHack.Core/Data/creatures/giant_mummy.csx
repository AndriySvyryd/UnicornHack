new CreatureVariant
{
    InitialLevel = 10,
    ArmorClass = 3,
    MagicResistance = 20,
    GenerationFrequency = Frequency.Uncommonly,
    Alignment = -7,
    Noise = ActorNoiseType.Moan,
    CorpseVariantName = "giant",
    Name = "giant mummy",
    Species = Species.Giant,
    SpeciesClass = SpeciesClass.Undead,
    MovementRate = 6,
    Size = Size.Huge,
    Weight = 2250,
    Nutrition = 350,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Humanoidness", "Breathlessness" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 }, { "SicknessResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 13 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Infect { } } }
    }
}
