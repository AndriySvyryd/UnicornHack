new Creature
{
    Name = "orc mummy",
    Species = Species.Orc,
    SpeciesClass = SpeciesClass.Undead,
    ArmorClass = 5,
    MagicResistance = 20,
    MovementDelay = 133,
    Weight = 1000,
    Size = Size.Medium,
    Nutrition = 100,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 4 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new Infect { } } }
    }
,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Humanoidness", "Breathlessness", "SicknessResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 } },
    InitialLevel = 5,
    CorpseName = "orc",
    GenerationFrequency = Frequency.Uncommonly,
    Alignment = -4,
    Noise = ActorNoiseType.Moan
}
