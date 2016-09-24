new CreatureVariant
{
    InitialLevel = 12,
    ArmorClass = 1,
    MagicResistance = 20,
    GenerationFrequency = Frequency.Uncommonly,
    Alignment = -4,
    Noise = ActorNoiseType.Moan,
    CorpseVariantName = "ettin",
    Name = "ettin mummy",
    Species = Species.Giant,
    SpeciesClass = SpeciesClass.Undead,
    MovementRate = 12,
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
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Infect { } } }
    }
}
