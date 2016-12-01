new CreatureVariant
{
    Name = "orc mummy",
    Species = Species.Orc,
    SpeciesClass = SpeciesClass.Undead,
    CorpseVariantName = "orc",
    InitialLevel = 5,
    ArmorClass = 5,
    MagicResistance = 20,
    MovementRate = 9,
    Weight = 1000,
    Size = Size.Medium,
    Nutrition = 100,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 4 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Infect { } } }
    }
,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Humanoidness", "Breathlessness", "SicknessResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 } },
    GenerationFrequency = Frequency.Uncommonly,
    Alignment = -4,
    Noise = ActorNoiseType.Moan
}
