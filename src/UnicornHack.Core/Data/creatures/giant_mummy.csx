new CreatureVariant
{
    Name = "giant mummy",
    Species = Species.Giant,
    SpeciesClass = SpeciesClass.Undead,
    CorpseVariantName = "giant",
    InitialLevel = 10,
    ArmorClass = 3,
    MagicResistance = 20,
    MovementRate = 6,
    Weight = 2250,
    Size = Size.Huge,
    Nutrition = 350,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 13 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Infect { } } }
    }
,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Humanoidness", "Breathlessness", "SicknessResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 } },
    GenerationFrequency = Frequency.Uncommonly,
    Alignment = -7,
    Noise = ActorNoiseType.Moan
}
