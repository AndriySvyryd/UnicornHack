new CreatureVariant
{
    Name = "human mummy",
    Species = Species.Human,
    SpeciesClass = SpeciesClass.Undead,
    CorpseVariantName = "human",
    InitialLevel = 6,
    ArmorClass = 4,
    MagicResistance = 20,
    MovementRate = 12,
    Weight = 1000,
    Size = Size.Medium,
    Nutrition = 200,
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
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Humanoidness", "Breathlessness", "SicknessResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 } },
    GenerationFrequency = Frequency.Sometimes,
    Alignment = -5,
    Noise = ActorNoiseType.Moan
}
