new CreatureVariant
{
    InitialLevel = 3,
    ArmorClass = 9,
    MagicResistance = 10,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.Stalking,
    Noise = ActorNoiseType.Moan,
    CorpseVariantName = "dwarf",
    Name = "dwarf zombie",
    Species = Species.Dwarf,
    SpeciesClass = SpeciesClass.Undead,
    MovementRate = 6,
    Size = Size.Medium,
    Weight = 900,
    Nutrition = 200,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Humanoidness", "Breathlessness", "Mindlessness" },
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
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Infect { } } }
    }
}
