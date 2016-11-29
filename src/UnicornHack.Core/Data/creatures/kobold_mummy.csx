new CreatureVariant
{
    InitialLevel = 3,
    ArmorClass = 6,
    MagicResistance = 20,
    GenerationFrequency = Frequency.Uncommonly,
    Alignment = -2,
    Noise = ActorNoiseType.Moan,
    CorpseVariantName = "kobold",
    Name = "kobold mummy",
    Species = Species.Kobold,
    SpeciesClass = SpeciesClass.Undead,
    MovementRate = 6,
    Size = Size.Small,
    Weight = 400,
    Nutrition = 50,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Humanoidness", "Breathlessness" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 }, { "SicknessResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Infect { } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 5 } } }
    }
}
