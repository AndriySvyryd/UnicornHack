new CreatureVariant
{
    InitialLevel = 6,
    ArmorClass = 4,
    MagicResistance = 20,
    GenerationFrequency = Frequency.Sometimes,
    Alignment = -5,
    Noise = ActorNoiseType.Moan,
    CorpseVariantName = "human",
    Name = "human mummy",
    Species = Species.Human,
    SpeciesClass = SpeciesClass.Undead,
    MovementRate = 12,
    Size = Size.Medium,
    Weight = 1000,
    Nutrition = 200,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Humanoidness", "Breathlessness" },
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
