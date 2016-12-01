new CreatureVariant
{
    Name = "gnome mummy",
    Species = Species.Gnome,
    SpeciesClass = SpeciesClass.Undead,
    CorpseVariantName = "gnome",
    InitialLevel = 4,
    ArmorClass = 6,
    MagicResistance = 20,
    MovementRate = 6,
    Weight = 650,
    Size = Size.Small,
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
    Alignment = -3,
    Noise = ActorNoiseType.Moan
}
