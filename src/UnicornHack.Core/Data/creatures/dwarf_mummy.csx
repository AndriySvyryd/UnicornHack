new CreatureVariant
{
    Name = "dwarf mummy",
    Species = Species.Dwarf,
    SpeciesClass = SpeciesClass.Undead,
    CorpseVariantName = "dwarf",
    InitialLevel = 5,
    ArmorClass = 5,
    MagicResistance = 30,
    MovementRate = 6,
    Weight = 900,
    Size = Size.Medium,
    Nutrition = 200,
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
    GenerationFrequency = Frequency.Sometimes,
    Alignment = -4,
    Noise = ActorNoiseType.Moan
}
