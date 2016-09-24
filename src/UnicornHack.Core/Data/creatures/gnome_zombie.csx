new CreatureVariant
{
    InitialLevel = 2,
    ArmorClass = 9,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Uncommonly,
    Behavior = MonsterBehavior.Stalking,
    Noise = ActorNoiseType.Moan,
    CorpseVariantName = "gnome",
    Name = "gnome zombie",
    Species = Species.Gnome,
    SpeciesClass = SpeciesClass.Undead,
    MovementRate = 6,
    Size = Size.Small,
    Weight = 650,
    Nutrition = 100,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Humanoidness", "Breathlessness", "Mindlessness" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 }, { "SicknessResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Infect { } } }
    }
}
