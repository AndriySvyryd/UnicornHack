new CreatureVariant
{
    InitialLevel = 4,
    ArmorClass = 6,
    MagicResistance = 20,
    GenerationFrequency = Frequency.Uncommonly,
    Alignment = -3,
    Noise = ActorNoiseType.Moan,
    CorpseVariantName = "gnome",
    Name = "gnome mummy",
    Species = Species.Gnome,
    SpeciesClass = SpeciesClass.Undead,
    MovementRate = 6,
    Size = Size.Small,
    Weight = 650,
    Nutrition = 100,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Humanoidness", "Breathlessness" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 }, { "SicknessResistance", 3 } },
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
}
