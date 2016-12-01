new CreatureVariant
{
    Name = "kobold mummy",
    Species = Species.Kobold,
    SpeciesClass = SpeciesClass.Undead,
    CorpseVariantName = "kobold",
    InitialLevel = 3,
    ArmorClass = 6,
    MagicResistance = 20,
    MovementRate = 6,
    Weight = 400,
    Size = Size.Small,
    Nutrition = 50,
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
,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Humanoidness", "Breathlessness", "SicknessResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 } },
    GenerationFrequency = Frequency.Uncommonly,
    Alignment = -2,
    Noise = ActorNoiseType.Moan
}
